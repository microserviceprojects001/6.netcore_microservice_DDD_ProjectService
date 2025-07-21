using Duende.IdentityServer;
using Duende.IdentityServer.Models;
using User.Identity.Services;
using System.Net.Http;

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
