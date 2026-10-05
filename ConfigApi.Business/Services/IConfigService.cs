using ConfigApi.Entities.Dtos;

namespace ConfigApi.Business.Services;

public interface IConfigService
{
    Task<ConfigResponse> GetByCompanyAsync(int companyId, string companyName);
}