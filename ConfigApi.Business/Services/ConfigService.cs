using ConfigApi.Context.Repositories;
using ConfigApi.Entities.Dtos;

namespace ConfigApi.Business.Services;

public class ConfigService : IConfigService
{
    private readonly IConfigRepository _configRepository;

    public ConfigService(IConfigRepository configRepository)
        => _configRepository = configRepository;

    public async Task<ConfigResponse> GetByCompanyAsync(int companyId, string companyName)
    {
        var configs = await _configRepository.GetByCompanyIdAsync(companyId);

        return new ConfigResponse
        {
            CompanyId = companyId,
            CompanyName = companyName,
            Settings = configs.ToDictionary(c => c.ConfigKey, c => c.ConfigValue)
        };
    }
}