<<<<<<< HEAD
﻿using ConfigApi.Context.Caching;
using ConfigApi.Context.Factory;
using ConfigApi.Context.Repositories;
using ConfigApi.Context.Search;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
=======
﻿using ConfigApi.Context.Factory;
using ConfigApi.Context.Repositories;
using Microsoft.Extensions.DependencyInjection;
>>>>>>> bcdf37d9dda12d4a7aca61ef0d9fa06bf7e79f43

namespace ConfigApi.Context;

public static class ContextServiceRegistration
{
<<<<<<< HEAD
    public static IServiceCollection AddContextServices(
        this IServiceCollection services, IConfiguration configuration)
=======
    public static IServiceCollection AddContextServices(this IServiceCollection services)
>>>>>>> bcdf37d9dda12d4a7aca61ef0d9fa06bf7e79f43
    {
        services.AddSingleton<IDbConnectionFactory, DbConnectionFactory>();

        services.AddScoped<ITenantRepository, TenantRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<ICartRepository, CartRepository>();
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IConfigRepository, ConfigRepository>();

<<<<<<< HEAD
        AddCaching(services, configuration);
        AddSearch(services, configuration);

        return services;
    }

    private static void AddCaching(IServiceCollection services, IConfiguration configuration)
    {
        var redis = configuration.GetSection("Redis");
        var enabled = redis.GetValue<bool>("Enabled");
        var connectionString = redis["Configuration"];

        if (!enabled || string.IsNullOrWhiteSpace(connectionString))
        {
            services.AddSingleton<ICacheService, NullCacheService>();
            return;
        }

        var instanceName = redis["InstanceName"] ?? "configapi:";

        try
        {
            var options = ConfigurationOptions.Parse(connectionString);
            options.AbortOnConnectFail = false;
            options.ConnectRetry = 3;
            options.ConnectTimeout = 3000;

            var multiplexer = ConnectionMultiplexer.Connect(options);

            services.AddSingleton<IConnectionMultiplexer>(multiplexer);
            services.AddSingleton<ICacheService>(sp => new RedisCacheService(
                multiplexer,
                sp.GetRequiredService<ILogger<RedisCacheService>>(),
                instanceName));
        }
        catch
        {
            services.AddSingleton<ICacheService, NullCacheService>();
        }
    }

    private static void AddSearch(IServiceCollection services, IConfiguration configuration)
    {
        var options = configuration.GetSection("Elasticsearch").Get<ElasticsearchOptions>()
                      ?? new ElasticsearchOptions();

        services.AddSingleton(options);

        if (!options.Enabled)
        {
            services.AddSingleton<IProductSearchIndex, NullProductSearchIndex>();
            services.AddScoped<IProductSearchQuery, NullProductSearchQuery>();
            return;
        }

        services.AddHttpClient(ElasticsearchOptions.HttpClientName, client =>
        {
            client.BaseAddress = new Uri(options.Uri);
            client.Timeout = TimeSpan.FromSeconds(30);
        });

        services.AddSingleton<IProductSearchIndex, ElasticProductSearchIndex>();
        services.AddScoped<IProductSearchQuery, ElasticProductSearchQuery>();
    }
=======
        return services;
    }
>>>>>>> bcdf37d9dda12d4a7aca61ef0d9fa06bf7e79f43
}
