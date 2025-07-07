// API/Program.cs
using ECommerce.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
// 注册服务
builder.Services.AddInfrastructure(configuration);

var app = builder.Build();
app.MapControllers();
app.Run();