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
            // 注册仓储
            services.AddScoped<IOrderRepository, OrderRepository>();

            // 注册领域事件发布器
            services.AddScoped<IDomainEventPublisher, DomainEventPublisher>();

            return services;
        }
    }
}