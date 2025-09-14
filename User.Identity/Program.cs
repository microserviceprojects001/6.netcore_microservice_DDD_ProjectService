using Duende.IdentityServer;
using Duende.IdentityServer.Models;
using User.Identity.Services;
using System.Net.Http;
using User.Identity.Dtos;
using Consul;
using Microsoft.Extensions.Options;
using Resilience;
using User.Identity.Infrastructure;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Services;
using User.Identity.Authentication;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddIdentityServer()
    .AddExtensionGrantValidator<SmsAuthCodeValidator>() // 添加自定义授权类型验证器
    .AddDeveloperSigningCredential()
    .AddInMemoryClients(Config.GetClients())
    .AddInMemoryIdentityResources(Config.GetIdentityResources())
    .AddInMemoryApiResources(Config.GetApiResource())
    .AddInMemoryApiScopes(Config.GetApiScopes());

builder.Services.AddHttpContextAccessor();
builder.Services.AddSingleton(typeof(ResilienceClientFactory), sp =>
{
    var logger = sp.GetRequiredService<ILogger<ResilienceHttplicent>>();
    var httpContextAccessor = sp.GetRequiredService<IHttpContextAccessor>();
    var retryCount = 5;
    var exceptionCountAllowedBeforeBreaking = 3;

    return new ResilienceClientFactory(logger, httpContextAccessor, retryCount, exceptionCountAllowedBeforeBreaking);
});
//注册全局单例IHttpClient
builder.Services.AddSingleton<IHttpClient>(sp =>
{

    return sp.GetRequiredService<ResilienceClientFactory>().GetResilienceHttplicent();
});

builder.Services.AddScoped<IAuthCodeService, TestAuthCodeService>();
builder.Services.AddScoped<IUserService, UserService>();

builder.Services.Configure<ServerDiscoveryConfig>(builder.Configuration.GetSection("ServerDiscovery"));

builder.Services.AddSingleton<IConsulClient>(provider =>
{
    var config = provider.GetRequiredService<IOptions<ServerDiscoveryConfig>>().Value;
    return new ConsulClient(cfg => cfg.Address = new Uri(config.Consul.HttpEndpoint));
});

builder.Services.AddScoped<IProfileService, ProfileService>();
// 修改为使用应用生命周期事件注册
builder.Services.AddSingleton<ConsulRegistrationService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

//app.UseHttpsRedirection();

app.UseIdentityServer();


app.MapControllers();
// 使用应用生命周期事件触发注册
//ASP.NET Core 在创建主机时自动注册了 IHostApplicationLifetime
var lifetime = app.Services.GetRequiredService<IHostApplicationLifetime>();
var consulService = app.Services.GetRequiredService<ConsulRegistrationService>();

lifetime.ApplicationStarted.Register(async () =>
{
    try
    {
        Console.WriteLine("应用已完全启动");
        await consulService.RegisterAsync(lifetime.ApplicationStopping);
    }
    catch (Exception ex)
    {
        var logger = app.Services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Consul 注册失败");
    }
});

lifetime.ApplicationStopping.Register(async () =>
{
    try
    {
        Console.WriteLine("应用正在停止...");
        await consulService.DeregisterAsync(lifetime.ApplicationStopping);
    }
    catch (Exception ex)
    {
        var logger = app.Services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Consul 注销失败");
    }
});

app.Run();
