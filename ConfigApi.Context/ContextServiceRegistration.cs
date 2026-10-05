using ConfigApi.Context.Factory;
using ConfigApi.Context.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace ConfigApi.Context;

public static class ContextServiceRegistration
{
    public static IServiceCollection AddContextServices(this IServiceCollection services)
    {
        services.AddSingleton<IDbConnectionFactory, DbConnectionFactory>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IConfigRepository, ConfigRepository>();
        services.AddScoped<ICompanyRepository, CompanyRepository>();
        return services;
    }
}