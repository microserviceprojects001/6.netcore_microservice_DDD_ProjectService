using Microsoft.Extensions.DependencyInjection;
using MediatR;
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(typeof(CreateOrderHandler).Assembly);

        services.AddAutoMapper(typeof(CreateOrderHandler).Assembly);
        return services;
    }
}