using ConfigApi.Context.Factory;
using ConfigApi.Entities.Entities;
using Dapper;

namespace ConfigApi.Context.Repositories;

public class ConfigRepository : IConfigRepository
{
    private readonly IDbConnectionFactory _factory;

    public ConfigRepository(IDbConnectionFactory factory) => _factory = factory;

    private const string GetByCompanySql = @"
        SELECT  Id, CompanyId, ConfigKey, ConfigValue, IsActive
        FROM    Configs
        WHERE   CompanyId = @CompanyId AND IsActive = 1;";

    public async Task<IEnumerable<Config>> GetByCompanyIdAsync(int companyId)
    {
        using var conn = _factory.Create();
        return await conn.QueryAsync<Config>(
            GetByCompanySql, new { CompanyId = companyId });
    }
}