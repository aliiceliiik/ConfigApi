using ConfigApi.Context.Factory;
using ConfigApi.Context.Repositories.Base;
using ConfigApi.Entities.Entities;
using Dapper;

namespace ConfigApi.Context.Repositories;

public interface IConfigRepository
{
    Task<IEnumerable<Config>> GetForTenantAsync();
}

public class ConfigRepository : TenantScopedRepository, IConfigRepository
{
    public ConfigRepository(IDbConnectionFactory f, ITenantProvider t) : base(f, t) { }

    private const string GetForTenantSql = @"
        SELECT  Id, TenantId, ConfigKey, ConfigValue, IsActive
        FROM    Configs
        WHERE   TenantId = @TenantId AND IsActive = 1;";

    public async Task<IEnumerable<Config>> GetForTenantAsync()
    {
        using var conn = Factory.Create();
        return await conn.QueryAsync<Config>(GetForTenantSql, new { TenantId });
    }
}
