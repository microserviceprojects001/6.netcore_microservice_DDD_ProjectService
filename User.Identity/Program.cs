using Duende.IdentityServer;
using Duende.IdentityServer.Models;
using User.Identity.Services;
using System.Net.Http;
using User.Identity.Dtos;
using Consul;
using Microsoft.Extensions.Options;

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

builder.Services.AddSingleton(new HttpClient());
builder.Services.AddScoped<IAuthCodeService, TestAuthCodeService>();
builder.Services.AddScoped<IUserService, UserService>();

builder.Services.Configure<ServerDiscoveryConfig>(builder.Configuration.GetSection("ServerDiscovery"));

builder.Services.AddSingleton<IConsulClient>(provider =>
{
    var config = provider.GetRequiredService<IOptions<ServerDiscoveryConfig>>().Value;
    return new ConsulClient(cfg => cfg.Address = new Uri(config.Consul.HttpEndpoint));
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

//app.UseHttpsRedirection();

app.UseIdentityServer();


app.MapControllers();

app.Run();
