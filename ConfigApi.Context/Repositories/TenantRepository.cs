using ConfigApi.Context.Factory;
using ConfigApi.Entities.Dtos;
using ConfigApi.Entities.Entities;
using Dapper;

namespace ConfigApi.Context.Repositories;

public class TenantRepository : ITenantRepository
{
    private readonly IDbConnectionFactory _factory;

    public TenantRepository(IDbConnectionFactory factory) => _factory = factory;

    private const string GetByDomainSql = @"
        SELECT  t.Id, t.Name, t.Slug, t.IsActive, t.CreatedAt
        FROM    Tenants t
        JOIN    TenantDomains d ON d.TenantId = t.Id
        WHERE   d.Domain = @Domain AND t.IsActive = 1;";

    public async Task<Tenant?> GetByDomainAsync(string domain)
    {
        using var conn = _factory.Create();
        return await conn.QuerySingleOrDefaultAsync<Tenant>(
            GetByDomainSql, new { Domain = domain });
    }

    public async Task<Tenant?> GetByIdAsync(Guid id)
    {
        using var conn = _factory.Create();
        return await conn.QuerySingleOrDefaultAsync<Tenant>(
            "SELECT Id, Name, Slug, IsActive, CreatedAt FROM Tenants WHERE Id = @Id;",
            new { Id = id });
    }

    public async Task<IEnumerable<Tenant>> GetAllAsync()
    {
        using var conn = _factory.Create();
        return await conn.QueryAsync<Tenant>(
            "SELECT Id, Name, Slug, IsActive, CreatedAt FROM Tenants ORDER BY Name;");
    }

    private const string GetListSql = @"
        SELECT  t.Id, t.Name, t.Slug, t.IsActive,
                (SELECT COUNT(*) FROM Users u  WHERE u.TenantId = t.Id) AS UserCount,
                (SELECT COUNT(*) FROM Orders o WHERE o.TenantId = t.Id) AS OrderCount
        FROM    Tenants t
        ORDER BY t.Name;";

    public async Task<IEnumerable<TenantListItemDto>> GetListAsync()
    {
        using var conn = _factory.Create();
        return await conn.QueryAsync<TenantListItemDto>(GetListSql);
    }
}
