// ServiceTokenProvider.cs
using IdentityModel.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Contact.API.Services
{
    public interface IServiceTokenProvider
    {
        Task<string> GetTokenAsync();
    }

    public class ServiceTokenProvider : IServiceTokenProvider
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<ServiceTokenProvider> _logger;

        public ServiceTokenProvider(
            HttpClient httpClient,
            IConfiguration configuration,
            ILogger<ServiceTokenProvider> logger)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;

            // 设置基础地址为网关地址
            _httpClient.BaseAddress = new Uri("https://localhost:5203");
        }

        public async Task<string> GetTokenAsync()
        {
            try
            {
                _logger.LogInformation("Requesting service token...");

                // 获取发现文档
                var discovery = await _httpClient.GetDiscoveryDocumentAsync();

                if (discovery.IsError)
                {
                    _logger.LogError("Discovery error: {Error}", discovery.Error);
                    throw new Exception($"Discovery error: {discovery.Error}");
                }

                // 请求客户端凭证令牌
                var response = await _httpClient.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
                {
                    Address = discovery.TokenEndpoint,
                    ClientId = _configuration["ServiceClient:ClientId"],
                    ClientSecret = _configuration["ServiceClient:ClientSecret"],
                    Scope = "user_api"
                });

                if (response.IsError)
                {
                    _logger.LogError("Token request failed: {Error}", response.Error);
                    throw new Exception($"Token request failed: {response.Error}");
                }

                _logger.LogInformation("Successfully obtained service token");
                return response.AccessToken;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get service token");
                throw;
            }
        }
    }
}