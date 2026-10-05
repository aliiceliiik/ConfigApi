using ConfigApi.Entities.Entities;

namespace ConfigApi.Context.Repositories;

public interface IConfigRepository
{
    Task<IEnumerable<Config>> GetByCompanyIdAsync(int companyId);
}