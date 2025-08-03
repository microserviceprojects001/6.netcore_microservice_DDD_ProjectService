using System.Net.Http;
using System.Threading.Tasks;
using Polly.Wrap;
using System.Collections.Concurrent;
using Polly;
using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System.Text;

namespace Resilience;

public class ResilienceHttplicent : IHttpClient
{
    private readonly HttpClient _httpClient;
    private readonly Func<string, IEnumerable<IAsyncPolicy<HttpResponseMessage>>> _policyCreators;
    private readonly ConcurrentDictionary<string, AsyncPolicyWrap<HttpResponseMessage>> _policyWrapperCache;
    private readonly ILogger<ResilienceHttplicent> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ResilienceHttplicent(
        Func<string, IEnumerable<IAsyncPolicy<HttpResponseMessage>>> policyCreator,
        ILogger<ResilienceHttplicent> logger,
        IHttpContextAccessor httpContextAccessor)
    {
        _httpClient = new HttpClient();
        _policyWrapperCache = new ConcurrentDictionary<string, AsyncPolicyWrap<HttpResponseMessage>>();
        _policyCreators = policyCreator;
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<HttpResponseMessage> PostAsync<T>(
        string url,
        T item,
        string authorizationToken = null,
        string requestid = null,
        string authorizationMethod = "Bearer")
    {
        Func<HttpRequestMessage> requestMessageFunc = () => CreateRequestMessage(HttpMethod.Post, url, item);
        return await DoPostAsync(HttpMethod.Post, url, requestMessageFunc, authorizationToken, requestid, authorizationMethod);
    }

    public async Task<HttpResponseMessage> PostAsync(
        string url,
        Dictionary<string, string> form,
        string authorizationToken = null,
        string requestid = null,
        string authorizationMethod = "Bearer")
    {
        Func<HttpRequestMessage> requestMessageFunc = () => CreateRequestMessage(HttpMethod.Post, url, form);
        return await DoPostAsync(HttpMethod.Post, url, requestMessageFunc, authorizationToken, requestid, authorizationMethod);
    }

    private async Task<HttpResponseMessage> DoPostAsync(
        HttpMethod method,
        string url,
        Func<HttpRequestMessage> requestMessageFunc,
        string authorizationToken,
        string requestid = null,
        string authorizationMethod = null)
    {
        if (method != HttpMethod.Post && method != HttpMethod.Put)
        {
            throw new ArgumentException("Method must be POST or PUT", nameof(method));
        }

        var origin = GetOriginFromUri(new Uri(url));
        return await HttpInvoker(origin, async () =>
        {
            var requestMessage = requestMessageFunc();
            SetAuthorizationHeader(requestMessage);

            if (authorizationToken != null)
            {
                requestMessage.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(
                    authorizationMethod, authorizationToken);
            }

            if (requestid != null)
            {
                requestMessage.Headers.Add("x-request-id", requestid);
            }

            var response = await _httpClient.SendAsync(requestMessage);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError($"Request to {url} failed with status code {response.StatusCode}");
            }
            return response;
        });
    }

    private async Task<HttpResponseMessage> HttpInvoker(string origin, Func<Task<HttpResponseMessage>> action)
    {
        var normalizedOrigin = NormalizeOrigin(origin);

        if (!_policyWrapperCache.TryGetValue(normalizedOrigin, out var policyWrap))
        {
            var policies = _policyCreators(normalizedOrigin).ToArray();
            policyWrap = Policy.WrapAsync(policies);
            _policyWrapperCache.TryAdd(normalizedOrigin, policyWrap);
        }

        return await policyWrap.ExecuteAsync((ctx) => action(), new Context(normalizedOrigin));
    }
    private string GetOrigin(string url)
    {
        var uri = new Uri(url);
        return $"{uri.Scheme}://{uri.Host}:{uri.Port}";
    }

    private HttpRequestMessage CreateRequestMessage<T>(HttpMethod method, string url, T item)
    {
        return new HttpRequestMessage(method, url)
        {
            Content = new StringContent(JsonConvert.SerializeObject(item), Encoding.UTF8, "application/json")
        };
    }

    private HttpRequestMessage CreateRequestMessage(HttpMethod method, string url, Dictionary<string, string> form)
    {

        return new HttpRequestMessage(method, url)
        {
            Content = new FormUrlEncodedContent(form)
        };
    }

    private static string NormalizeOrigin(string origin)
    {
        return origin?.Trim()?.ToLower();
    }

    private static string GetOriginFromUri(Uri uri)
    {
        return $"{uri.Scheme}://{uri.Host}:{uri.Port}";
    }

    private void SetAuthorizationHeader(HttpRequestMessage requestMessage)
    {
        var authorizationHeader = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"].ToString();
        if (!string.IsNullOrEmpty(authorizationHeader))
        {
            requestMessage.Headers.Add("Authorization", new List<string> { authorizationHeader });
        }
    }
}