using Contact.API.Data;
using Microsoft.Extensions.Options;
using Contact.API;
using Contact.API.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer; // 添加这个
using Microsoft.IdentityModel.Tokens; // 添加这个
using System.IdentityModel.Tokens.Jwt;

var builder = WebApplication.CreateBuilder(args);

// 清除默认的声明类型映射
JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = "https://localhost:5203"; // 网关地址
        options.RequireHttpsMetadata = true; // 开发环境可以设为false
        options.Audience = "contact_api";
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

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// 添加认证中间件
app.UseAuthentication(); // 必须在 UseAuthorization 之前
//app.UseAuthorization();

app.MapControllers();

app.Run();
