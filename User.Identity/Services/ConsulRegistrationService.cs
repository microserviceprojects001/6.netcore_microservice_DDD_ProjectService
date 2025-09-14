using Consul;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using User.Identity.Dtos;

namespace User.Identity.Services;

public class ConsulRegistrationService
{
    private readonly IConsulClient _consulClient;
    private readonly ServerDiscoveryConfig _consulConfig;
    private readonly ILogger<ConsulRegistrationService> _logger;
    private readonly IServer _server;
    private readonly List<string> _registeredIds = new List<string>();

    public ConsulRegistrationService(
        IConsulClient consulClient,
        IOptions<ServerDiscoveryConfig> consulConfig,
        ILogger<ConsulRegistrationService> logger,
        IServer server)
    {
        _consulClient = consulClient;
        _consulConfig = consulConfig.Value;
        _logger = logger;
        _server = server;
    }

    public async Task RegisterAsync(CancellationToken cancellationToken)
    {
        var serverAddresses = _server.Features.Get<IServerAddressesFeature>();

        if (serverAddresses == null)
        {
            _logger.LogError("无法获取服务器地址特征");
            return;
        }

        if (!serverAddresses.Addresses.Any())
        {
            _logger.LogError("服务器地址集合为空");
            return;
        }

        _logger.LogInformation($"检测到服务器地址: {string.Join(", ", serverAddresses.Addresses)}");

        foreach (var address in serverAddresses.Addresses)
        {
            try
            {
                var sanitizedAddress = address
                    .Replace("0.0.0.0", Dns.GetHostName())
                    .Replace("*", Dns.GetHostName())
                    .Replace("+", Dns.GetHostName())
                    .Replace("[::]", Dns.GetHostName());

                var uri = new Uri(sanitizedAddress);
                var host = uri.Host;
                var port = uri.Port;

                var serviceId = $"{_consulConfig.IdentityServiceName}-{host}:{port}";
                _registeredIds.Add(serviceId);

                var scheme = _consulConfig.UseHttps ? "https" : "http";

                var registration = new AgentServiceRegistration
                {
                    ID = serviceId,
                    Name = _consulConfig.IdentityServiceName,
                    Address = host,
                    Port = port,
                    Tags = _consulConfig.UseHttps ? new[] { "https" } : Array.Empty<string>(),
                    Check = new AgentServiceCheck
                    {
                        HTTP = $"{scheme}://{host}:{port}/HealthCheck",
                        Interval = TimeSpan.FromSeconds(10),
                        Timeout = TimeSpan.FromSeconds(5),
                        DeregisterCriticalServiceAfter = TimeSpan.FromSeconds(30)
                    }
                };

                await _consulClient.Agent.ServiceRegister(registration, cancellationToken);
                _logger.LogInformation($"✅ 已注册服务到Consul: {serviceId}");
                _logger.LogInformation($"   健康检查: {registration.Check.HTTP}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ 注册服务到Consul失败: {address}");
            }
        }
    }

    public async Task DeregisterAsync(CancellationToken cancellationToken)
    {
        foreach (var serviceId in _registeredIds)
        {
            try
            {
                await _consulClient.Agent.ServiceDeregister(serviceId, cancellationToken);
                _logger.LogInformation($"已注销Consul服务: {serviceId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"注销Consul服务失败: {serviceId}");
            }
        }
    }
}