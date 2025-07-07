
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ECommerce.Infrastructure.Persistence;
using ECommerce.Domain.Interfaces;
using ECommerce.Infrastructure.Services;
using ECommerce.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        services.AddDbContext<OrderDbContext>(options =>
                        options.UseSqlite(config.GetConnectionString("Default")));

        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IEmailService, EmailService>();

        return services;
    }
}
