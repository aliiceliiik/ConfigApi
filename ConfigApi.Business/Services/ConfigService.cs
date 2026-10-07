using ConfigApi.Business.Tenancy;
using ConfigApi.Context.Caching;
using ConfigApi.Context.Repositories;
using ConfigApi.Entities.Dtos;

namespace ConfigApi.Business.Services;

public interface IConfigService
{
    Task<ConfigResponse> GetAsync();
}

public class ConfigService : IConfigService
{
    private static readonly TimeSpan Ttl = TimeSpan.FromMinutes(30);

    private readonly IConfigRepository _configs;
    private readonly ICacheService _cache;
    private readonly ITenantContext _tenant;

    public ConfigService(IConfigRepository configs, ICacheService cache, ITenantContext tenant)
    {
        _configs = configs;
        _cache = cache;
        _tenant = tenant;
    }

    public async Task<ConfigResponse> GetAsync()
    {
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
    }
}
