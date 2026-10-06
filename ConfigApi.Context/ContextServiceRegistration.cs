using ConfigApi.Context.Factory;
using ConfigApi.Context.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace ConfigApi.Context;

public static class ContextServiceRegistration
{
    public static IServiceCollection AddContextServices(this IServiceCollection services)
    {
        services.AddSingleton<IDbConnectionFactory, DbConnectionFactory>();

        services.AddScoped<ITenantRepository, TenantRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<ICartRepository, CartRepository>();
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IConfigRepository, ConfigRepository>();

        return services;
    }
}
