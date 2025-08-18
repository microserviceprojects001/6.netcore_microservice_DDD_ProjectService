using Contact.API.Data;
using Microsoft.Extensions.Options;
using Contact.API;
using Contact.API.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.


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

app.UseAuthorization();

app.MapControllers();

app.Run();
