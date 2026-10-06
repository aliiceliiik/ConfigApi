using ConfigApi.Business.Tenancy;
using ConfigApi.Context.Repositories;
using ConfigApi.Entities.Dtos;

namespace ConfigApi.Business.Services;

public interface IConfigService
{
    Task<ConfigResponse> GetAsync();
}

public class ConfigService : IConfigService
{
    private readonly IConfigRepository _configs;
    private readonly ITenantContext _tenant;

    public ConfigService(IConfigRepository configs, ITenantContext tenant)
    {
        _configs = configs;
        _tenant = tenant;
    }

    public async Task<ConfigResponse> GetAsync()
    {
        var configs = await _configs.GetForTenantAsync();

        return new ConfigResponse
        {
            TenantId = _tenant.TenantId,
            TenantName = _tenant.TenantName,
            Settings = configs.ToDictionary(c => c.ConfigKey, c => c.ConfigValue)
        };
    }
}
