using ConfigApi.Business.Tenancy;
<<<<<<< HEAD
using ConfigApi.Context.Caching;
=======
>>>>>>> bcdf37d9dda12d4a7aca61ef0d9fa06bf7e79f43
using ConfigApi.Context.Repositories;
using ConfigApi.Entities.Dtos;

namespace ConfigApi.Business.Services;

public interface IConfigService
{
    Task<ConfigResponse> GetAsync();
}

public class ConfigService : IConfigService
{
<<<<<<< HEAD
    private static readonly TimeSpan Ttl = TimeSpan.FromMinutes(30);

    private readonly IConfigRepository _configs;
    private readonly ICacheService _cache;
    private readonly ITenantContext _tenant;

    public ConfigService(IConfigRepository configs, ICacheService cache, ITenantContext tenant)
    {
        _configs = configs;
        _cache = cache;
=======
    private readonly IConfigRepository _configs;
    private readonly ITenantContext _tenant;

    public ConfigService(IConfigRepository configs, ITenantContext tenant)
    {
        _configs = configs;
>>>>>>> bcdf37d9dda12d4a7aca61ef0d9fa06bf7e79f43
        _tenant = tenant;
    }

    public async Task<ConfigResponse> GetAsync()
    {
<<<<<<< HEAD
        var key = CacheKeys.Config(_tenant.TenantId);

        return await _cache.GetOrSetAsync(key, Ttl, async () =>
        {
            var configs = await _configs.GetForTenantAsync();

            return new ConfigResponse
            {
                TenantId = _tenant.TenantId,
                TenantName = _tenant.TenantName,
                Settings = configs.ToDictionary(c => c.ConfigKey, c => c.ConfigValue)
            };
        });
=======
        var configs = await _configs.GetForTenantAsync();

        return new ConfigResponse
        {
            TenantId = _tenant.TenantId,
            TenantName = _tenant.TenantName,
            Settings = configs.ToDictionary(c => c.ConfigKey, c => c.ConfigValue)
        };
>>>>>>> bcdf37d9dda12d4a7aca61ef0d9fa06bf7e79f43
    }
}
