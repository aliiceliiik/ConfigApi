using ConfigApi.Business.Security;
using ConfigApi.Business.Services;
using ConfigApi.Business.Tenancy;
using ConfigApi.Context;
using ConfigApi.Context.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace ConfigApi.Business;

public static class BusinessServiceRegistration
{
    public static IServiceCollection AddBusinessServices(this IServiceCollection services)
    {
        services.AddContextServices();

        services.AddHttpContextAccessor();

        services.AddScoped<TenantContext>();
        services.AddScoped<ITenantContext>(sp => sp.GetRequiredService<TenantContext>());
        services.AddScoped<ITenantProvider>(sp => sp.GetRequiredService<TenantContext>());

        services.AddSingleton<ITokenService, TokenService>();

        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<ICartService, CartService>();
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<IConfigService, ConfigService>();
        services.AddScoped<IAdminService, AdminService>();

        return services;
    }
}
