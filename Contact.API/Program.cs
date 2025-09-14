using Contact.API.Data;
using Microsoft.Extensions.Options;
using Contact.API;
using Contact.API.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer; // 添加这个
using Microsoft.IdentityModel.Tokens; // 添加这个
using System.IdentityModel.Tokens.Jwt;
using Contact.API.Dtos;
using Consul;
using Resilience;
using Contact.API.Infrastructure;
using Contact.API.Configuration;

var builder = WebApplication.CreateBuilder(args);

// 清除默认的声明类型映射
JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = "https://localhost:5203"; // 需要是网关地址
        options.RequireHttpsMetadata = true;
        options.Audience = "contactResource";
        options.SaveToken = true;
        // options.TokenValidationParameters = new TokenValidationParameters
        // {
        //     ValidateIssuer = true,
        //     ValidIssuer = "https://localhost:5203"
        // };
    });


// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// 1. 配置AppSettings绑定
builder.Services.Configure<AppSettings>(builder.Configuration.GetSection("AppSettings"));

builder.Services.Configure<ClientSettings>(builder.Configuration.GetSection("ClientSettings"));

// 2. 注册MongoDB Context（单例模式）
builder.Services.AddSingleton<ContactContext>(provider =>
{
    var settings = provider.GetRequiredService<IOptionsMonitor<AppSettings>>();
    return new ContactContext(settings);
});

// 3. 注册仓储接口实现
builder.Services.AddScoped<IContactRepository, MongoContactRepository>();
builder.Services.AddScoped<IContactApplyRequestRepository, MongoContactApplyRequestRepository>();
builder.Services.AddScoped<IUserService, UserService>();




builder.Services.AddControllers();

builder.Services.Configure<ServerDiscoveryConfig>(builder.Configuration.GetSection("ServerDiscovery"));

builder.Services.AddSingleton<IConsulClient>(provider =>
{
    var config = provider.GetRequiredService<IOptions<ServerDiscoveryConfig>>().Value;
    return new ConsulClient(cfg => cfg.Address = new Uri(config.Consul.HttpEndpoint));
});

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
// client token service test
//builder.Services.AddScoped<IServiceTokenProvider, ServiceTokenProvider>();
builder.Services.AddScoped<IClientAuthTestService, ClientAuthTestService>();
// ... existing code ...

// ... existing code ...

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("contactResource", policy =>
      policy.RequireAssertion(context =>
      {
          var audienceClaims = context.User.FindAll(c => c.Type == "aud");
          return audienceClaims.Any(c => c.Value == "contactResource");
      }));
    // ========== 保留现有的基于Scope的策略 ==========
    // options.AddPolicy("Contact.Read", policy =>
    //     policy.RequireScope("contact.read", "contact.manage", "contact.admin"));

    // options.AddPolicy("Contact.Write", policy =>
    //     policy.RequireScope("contact.write", "contact.manage", "contact.admin"));

    options.AddPolicy("Contact.Manage", policy =>
        policy.RequireScope("contact.manage", "contact.admin"));

    options.AddPolicy("Contact.Admin", policy =>
        policy.RequireScope("contact.admin"));

    // ========== 新增基于角色的策略 ==========
    options.AddPolicy("Contact.Read.Role", policy =>
        policy.RequireRole("ContactAdmin", "ContactManager", "ContentEditor", "ContactViewer"));

    options.AddPolicy("Contact.Write.Role", policy =>
        policy.RequireRole("ContactAdmin", "ContactManager", "ContentEditor"));

    options.AddPolicy("Contact.Manage.Role", policy =>
        policy.RequireRole("ContactAdmin", "ContactManager"));

    options.AddPolicy("Contact.Admin.Role", policy =>
        policy.RequireRole("ContactAdmin"));

    // ========== 混合策略：同时满足Scope和Role要求 ==========
    options.AddPolicy("Contact.Read", policy =>
        policy.RequireAssertion(context =>
        {
            var hasScope = context.User.HasScope("contact.read", "contact.manage", "contact.admin");
            var hasRole = context.User.IsInRole("ContactAdmin", "ContactManager", "ContentEditor", "ContactViewer");
            return hasScope && hasRole; // 必须同时满足
        }));

    options.AddPolicy("Contact.Write", policy =>
        policy.RequireAssertion(context =>
        {
            var hasScope = context.User.HasScope("contact.write", "contact.manage", "contact.admin");
            var hasRole = context.User.IsInRole("ContactAdmin", "ContactManager", "ContentEditor");
            return hasScope && hasRole;
        }));
});

// ... rest of code ...

// ... rest of code ...
// 修改为使用应用生命周期事件注册
builder.Services.AddSingleton<ConsulRegistrationService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// 添加认证中间件
app.UseAuthentication(); // 必须在 UseAuthorization 之前
app.UseAuthorization();

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
