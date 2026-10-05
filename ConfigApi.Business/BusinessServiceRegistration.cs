using ConfigApi.Business.Security;
using ConfigApi.Business.Services;
using ConfigApi.Context;
using Microsoft.Extensions.DependencyInjection;

namespace ConfigApi.Business;

public static class BusinessServiceRegistration
{
    public static IServiceCollection AddBusinessServices(this IServiceCollection services)
    {
        services.AddContextServices();          // alt katmanı burada zincirliyoruz
        services.AddSingleton<ITokenService, TokenService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IConfigService, ConfigService>();

        return services;
    }
}