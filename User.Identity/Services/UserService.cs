using System.Net.Http;
using System;
using System.Threading.Tasks;
using Consul;
using User.Identity.Dtos;
using Microsoft.Extensions.Options;

namespace User.Identity.Services;

public class UserService : IUserService
{
    private readonly HttpClient _httpClient;
    private readonly IConsulClient _consulClient;
    private readonly ServerDiscoveryConfig _options;
    private readonly string _userServiceUrl = "https://localhost:5201";
    public UserService(
          HttpClient httpClient,
          IConsulClient consulClient,
          IOptions<ServerDiscoveryConfig> options)
    {
        _httpClient = httpClient;
        _consulClient = consulClient;
        _options = options.Value;
    }

    public async Task<int> CheckOrCreate(string phone)
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
            Path = "/api/users/check-or-create"
        }.Uri;

        var form = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("phone", phone)
        });

        var response = await _httpClient.PostAsync(uri, form);

        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadAsStringAsync();
            return int.Parse(result);
        }

        return -1;
    }
}