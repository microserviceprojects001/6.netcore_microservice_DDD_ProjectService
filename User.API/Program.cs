using Microsoft.EntityFrameworkCore;
using User.API.Data;
using User.API.Models;
using User.API.Filters;
using Consul;
using User.API.Dtos;
using Microsoft.Extensions.Options;
using User.API.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer; // 添加这个
using Microsoft.IdentityModel.Tokens; // 添加这个
using System.IdentityModel.Tokens.Jwt;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers(options =>
{
    options.Filters.Add<GlobalExceptionFilter>();
}).AddNewtonsoftJson();

builder.Services.AddOpenApi();

var connectionString = builder.Configuration.GetConnectionString("MySQL");
Console.WriteLine($"DB Server: {connectionString}");

builder.Services.AddDbContext<UserContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString))
);

// Consul配置
builder.Services.Configure<ServerDiscoveryConfig>(builder.Configuration.GetSection("ServerDiscovery"));
builder.Services.AddSingleton<IConsulClient>(provider =>
{
    var config = provider.GetRequiredService<IOptions<ServerDiscoveryConfig>>().Value;
    return new ConsulClient(cfg => cfg.Address = new Uri(config.Consul.HttpEndpoint));
});

// 修改为使用应用生命周期事件注册
builder.Services.AddSingleton<ConsulRegistrationService>();
// 移除之前的 HostedService 注册

builder.Services.AddHealthChecks();

JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = "https://localhost:5203"; // 网关地址
        options.RequireHttpsMetadata = true; // 开发环境可以设为false
        options.Audience = "user_api";
        // options.TokenValidationParameters = new TokenValidationParameters
        // {
        //     ValidateIssuer = true,
        //     ValidIssuer = "https://localhost:5203"
        // };
    });

builder.Services.AddAuthorization(options =>
{
    //编译阶段就加载执行了
    // options.AddPolicy("RequireUserApiScope", policy =>
    //     {
    //         policy.RequireAuthenticatedUser();
    //         policy.RequireClaim("scope", "user_api");
    //     });
    options.AddPolicy("user_api", policy =>
      policy.RequireAssertion(context =>
      {
          var audienceClaims = context.User.FindAll(c => c.Type == "aud");
          return audienceClaims.Any(c => c.Value == "user_api");
      }));

    options.AddPolicy("RequireUserApiScope", policy =>
       policy.RequireAssertion(context =>
       {
           // 修正：直接从 User 中获取 scope 声明
           var scopeClaims = context.User.FindAll(c => c.Type == "scope");
           var userScopes = scopeClaims.SelectMany(c => c.Value.Split(' ')).ToList();

           // 检查是否包含 user_api scope
           return userScopes.Contains("user_api");
       }));
});

var app = builder.Build();

// 初始化数据库
UserContextSeed.Seed(app);

// 配置HTTP请求管道
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/HealthCheck");

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