using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Polly;
using Resilience;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;

namespace User.Identity.Infrastructure;

public class ResilienceClientFactory
{
    private ILogger<ResilienceHttplicent> _logger;
    private IHttpContextAccessor _httpContextAccessor;

    // 重试次数
    private int _retryCount = 3;

    // 熔断之前 允许的异常次数
    private int _exceptionCountAllowedBeforeBreaking = 5;
    public ResilienceClientFactory(
        ILogger<ResilienceHttplicent> logger,
        IHttpContextAccessor httpContextAccessor,
        int retryCount = 3,
        int exceptionCountAllowedBeforeBreaking = 5)
    {
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
        _retryCount = retryCount;
        _exceptionCountAllowedBeforeBreaking = exceptionCountAllowedBeforeBreaking;
    }

    public ResilienceHttplicent GetResilienceHttplicent() => new ResilienceHttplicent(
         origin => CreatePolicies(origin),
         _logger,
         _httpContextAccessor
     );

    private IEnumerable<IAsyncPolicy<HttpResponseMessage>> CreatePolicies(string origin)
    {
        return new IAsyncPolicy<HttpResponseMessage>[]
        {
            CreateRetryPolicy(),
            CreateCircuitBreakerPolicy()
        };
    }

    private IAsyncPolicy<HttpResponseMessage> CreateRetryPolicy()
    {
        return Policy<HttpResponseMessage>
            .Handle<HttpRequestException>()
            .OrResult(r => (int)r.StatusCode >= 500)
            .WaitAndRetryAsync(
                _retryCount,
                retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                (outcome, delay, retryCount, context) =>
                {
                    _logger.LogWarning($"第 {retryCount} 次重试 of {context.PolicyKey} " +
                                     $"due to {outcome.Exception?.Message ?? outcome.Result.StatusCode.ToString()}");
                });
    }

    private IAsyncPolicy<HttpResponseMessage> CreateCircuitBreakerPolicy()
    {
        return Policy<HttpResponseMessage>
            .Handle<HttpRequestException>()
            .OrResult(r => (int)r.StatusCode >= 500)
            .CircuitBreakerAsync(
                _exceptionCountAllowedBeforeBreaking,
                TimeSpan.FromMinutes(1),
                onBreak: (outcome, duration) =>
                {
                    _logger.LogError($"熔断器已触发，原因：{outcome.Exception?.Message ?? outcome.Result.StatusCode.ToString()}");
                },
                onReset: () =>
                {
                    _logger.LogInformation("熔断器已重置");
                },
                onHalfOpen: () =>
                {
                    _logger.LogInformation("熔断器半开启状态");
                });
    }
}