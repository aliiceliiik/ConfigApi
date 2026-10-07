using ConfigApi.Business.Security;
using ConfigApi.Business.Services;
using ConfigApi.Business.Tenancy;
using ConfigApi.Context;
using ConfigApi.Context.Repositories;
<<<<<<< HEAD
using Microsoft.Extensions.Configuration;
=======
>>>>>>> bcdf37d9dda12d4a7aca61ef0d9fa06bf7e79f43
using Microsoft.Extensions.DependencyInjection;

namespace ConfigApi.Business;

public static class BusinessServiceRegistration
{
<<<<<<< HEAD
    public static IServiceCollection AddBusinessServices(
        this IServiceCollection services, IConfiguration configuration)
    {
        services.AddContextServices(configuration);
=======
    public static IServiceCollection AddBusinessServices(this IServiceCollection services)
    {
        services.AddContextServices();
>>>>>>> bcdf37d9dda12d4a7aca61ef0d9fa06bf7e79f43

        services.AddHttpContextAccessor();

        services.AddScoped<TenantContext>();
        services.AddScoped<ITenantContext>(sp => sp.GetRequiredService<TenantContext>());
        services.AddScoped<ITenantProvider>(sp => sp.GetRequiredService<TenantContext>());

        services.AddSingleton<ITokenService, TokenService>();

        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IProductService, ProductService>();
<<<<<<< HEAD
        services.AddScoped<IProductSyncService, ProductSyncService>();
=======
>>>>>>> bcdf37d9dda12d4a7aca61ef0d9fa06bf7e79f43
        services.AddScoped<ICartService, CartService>();
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<IConfigService, ConfigService>();
        services.AddScoped<IAdminService, AdminService>();

        return services;
    }
}
