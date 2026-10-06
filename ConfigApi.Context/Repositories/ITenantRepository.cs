using ConfigApi.Entities.Dtos;
using ConfigApi.Entities.Entities;

namespace ConfigApi.Context.Repositories;

public interface ITenantRepository
{
    Task<Tenant?> GetByDomainAsync(string domain);
    Task<Tenant?> GetByIdAsync(Guid id);
    Task<IEnumerable<Tenant>> GetAllAsync();
    Task<IEnumerable<TenantListItemDto>> GetListAsync();
}
