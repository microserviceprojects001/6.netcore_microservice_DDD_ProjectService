// Contact.API/Services/ClientAuthTestService.cs
using Contact.API.Data;
using Microsoft.Extensions.Options;
using Contact.API.Dtos;
using Consul;
using Resilience;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using Contact.API.Configuration;

namespace Contact.API.Services
{
    public interface IClientAuthTestService
    {
        Task<string> TestPublicEndpoint();
        Task<string> TestProtectedEndpoint();
        Task<string> TestUserApiOnlyEndpoint();
        Task<string> TestRequireUserApiScopeEndpoint();
        Task<string> TestTokenAcquisition();
    }

    public class ClientAuthTestService : IClientAuthTestService
    {
        private readonly IHttpClient _httpClient;
        private readonly IConsulClient _consulClient;
        private readonly ServerDiscoveryConfig _options;
        private readonly ClientSettings _clientSettings;
        private readonly ILogger<ClientAuthTestService> _logger;

        public ClientAuthTestService(
            IHttpClient httpClient,
            IConsulClient consulClient,
            IOptions<ServerDiscoveryConfig> options,
            IOptions<ClientSettings> clientSettings,
            ILogger<ClientAuthTestService> logger)
        {
            _httpClient = httpClient;
            _consulClient = consulClient;
            _options = options.Value;
            _logger = logger;
            _clientSettings = clientSettings.Value;
        }

        public async Task<string> TestTokenAcquisition()
        {
            try
            {
                _logger.LogInformation("Testing token acquisition...");

                // 获取服务令牌
                var token = await GetServiceTokenAsync();

                _logger.LogInformation("Successfully obtained service token");
                return $"Token acquisition successful. Token: {token}";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Token acquisition test failed");
                return $"Token acquisition test failed: {ex.Message}";
            }
        }

        public async Task<string> TestPublicEndpoint()
        {
            return await CallTestEndpoint("/api/test/client-auth/public");
        }

        public async Task<string> TestProtectedEndpoint()
        {
            return await CallTestEndpoint("/api/test/client-auth/protected");
        }

        public async Task<string> TestUserApiOnlyEndpoint()
        {
            return await CallTestEndpoint("/api/test/client-auth/user-api-only");
        }

        public async Task<string> TestRequireUserApiScopeEndpoint()
        {
            return await CallTestEndpoint("/api/test/client-auth/require-user-api-scope");
        }

        private async Task<string> CallTestEndpoint(string endpointPath)
        {
            try
            {
                _logger.LogInformation("Calling test endpoint: {Endpoint}", endpointPath);

                // 从Consul获取User.API服务地址
                var services = await _consulClient.Health.Service(_options.UserServiceName, tag: null, passingOnly: true);
                var service = services.Response.FirstOrDefault();

                if (service == null)
                {
                    throw new Exception($"No healthy instances of {_options.UserServiceName} found");
                }

                // 构建请求URL
                var uri = new UriBuilder
                {
                    Scheme = service.Service.Tags.Contains("https") ? "https" : "http",
                    Host = service.Service.Address,
                    Port = service.Service.Port,
                    Path = endpointPath
                }.ToString();

                // 获取服务令牌
                var token = await GetServiceTokenAsync();

                // 调用测试端点
                var response = await _httpClient.GetStringAsync(uri, token);

                _logger.LogInformation("Test endpoint call successful: {Endpoint}", endpointPath);
                return $"Test endpoint call successful. Response: {response}";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Test endpoint call failed: {Endpoint}", endpointPath);
                return $"Test endpoint call failed: {ex.Message}";
            }
        }

        private async Task<string> GetServiceTokenAsync()
        {
            try
            {
                // 从Consul获取IdentityServer服务地址
                var services = await _consulClient.Health.Service(_options.IdentityServiceName, tag: null, passingOnly: true);
                var service = services.Response.FirstOrDefault();

                if (service == null)
                {
                    throw new Exception("No healthy instances of IdentityServer found");
                }

                // 构建IdentityServer地址
                var identityServerUrl = new UriBuilder
                {
                    Scheme = service.Service.Tags.Contains("https") ? "https" : "http",
                    Host = service.Service.Address,
                    Port = service.Service.Port
                }.ToString();

                // 手动创建令牌请求
                var requestContent = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("grant_type", "client_credentials"),
                    new KeyValuePair<string, string>("client_id", _clientSettings.ClientId),
                    new KeyValuePair<string, string>("client_secret", _clientSettings.ClientSecret),
                    new KeyValuePair<string, string>("scope", "user_api")
                });

                // 创建临时HttpClient用于获取令牌
                using var httpClient = new HttpClient();
                httpClient.BaseAddress = new Uri(identityServerUrl);

                var response = await httpClient.PostAsync("/connect/token", requestContent);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    throw new Exception($"Token request failed: {response.StatusCode}. Error: {errorContent}");
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                var tokenResponse = JsonConvert.DeserializeObject<TokenResponse>(responseContent);

                return tokenResponse.AccessToken;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get service token");
                throw;
            }
        }

        // 用于反序列化令牌响应的辅助类
        private class TokenResponse
        {
            [JsonProperty("access_token")]
            public string AccessToken { get; set; }

            [JsonProperty("expires_in")]
            public int ExpiresIn { get; set; }

            [JsonProperty("token_type")]
            public string TokenType { get; set; }

            [JsonProperty("scope")]
            public string Scope { get; set; }
        }
    }
}