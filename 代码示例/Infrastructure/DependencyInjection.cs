using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

// Infrastructure/DependencyInjection.cs
using Application.Common.Interfaces;
using Infrastructure.Repositories;
using Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services)
        {
            // ✅ Scoped生命周期：每个HTTP请求一个实例
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString),
                ServiceLifetime.Scoped);  // 关键配置！

            services.AddScoped<IOrderRepository, OrderRepository>();      // Scoped
            services.AddScoped<IDomainEventPublisher, DomainEventPublisher>(); // Scoped

            return services;
        }
    }
}