using Contact.API.Data;
using Microsoft.Extensions.Options;
using Contact.API;
using Contact.API.Configuration;
using Consul;
using Resilience;
using Newtonsoft.Json;
using Contact.API.Dtos;

namespace Contact.API.Services;

public class UserService : IUserService
{
    private readonly IHttpClient _httpClient;
    private readonly IConsulClient _consulClient;
    private readonly ServerDiscoveryConfig _options;

    private readonly IHttpContextAccessor _httpContextAccessor;
    //private readonly string _userServiceUrl = "https://localhost:5201";
    private readonly ILogger<UserService> _logger;
    public UserService(
          IHttpClient httpClient,
          IConsulClient consulClient,
          IOptions<ServerDiscoveryConfig> options,
          ILogger<UserService> logger,
          IHttpContextAccessor httpContextAccessor)
    {
        _httpClient = httpClient;
        _consulClient = consulClient;
        _options = options.Value;
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
    }
    // Implement methods to interact with the contact repository
    public async Task<UserIdentity> GetBaseUserInfoAsync(int userId)
    {
        // 从Consul获取服务地址
        var services = await _consulClient.Health.Service(_options.UserServiceName, tag: null, passingOnly: true);
        var service = services.Response.FirstOrDefault();
        if (service == null)
        {
            throw new Exception($"No healthy instances of {_options.UserServiceName} found");
        }

        var uri = new UriBuilder
        {
            Scheme = service.Service.Tags.Contains("https") ? "https" : "http",
            Host = service.Service.Address,
            Port = service.Service.Port,
            Path = $"/api/users/baseinfo/{userId}"
        }.ToString();

        try
        {
            // 从 HTTP 上下文中获取 token
            var authorizationHeader = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"].ToString();
            string authorizationToken = null;

            if (!string.IsNullOrEmpty(authorizationHeader) && authorizationHeader.StartsWith("Bearer "))
            {
                authorizationToken = authorizationHeader.Substring("Bearer ".Length);
            }
            var response = await _httpClient.GetStringAsync(uri, authorizationToken);
            if (string.IsNullOrEmpty(response))
            {
                return null;
            }
            var userInfo = JsonConvert.DeserializeObject<UserIdentity>(response);
            _logger.LogInformation($"Completed check-or-create with userID: {userInfo.UserId}");
            return userInfo;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling check-or-create");
            throw ex;
        }
        return null;
    }
}