using ConfigApi.Context.Factory;
using Dapper;

namespace ConfigApi.Context.Repositories;

public class CompanyRepository : ICompanyRepository
{
    private readonly IDbConnectionFactory _factory;

    public CompanyRepository(IDbConnectionFactory factory) => _factory = factory;

    private const string GetAllowedDomainsSql = @"
        SELECT  AllowedDomains
        FROM    Companies
        WHERE   Id = @CompanyId AND IsActive = 1;";

    public async Task<string?> GetAllowedDomainsAsync(int companyId)
    {
        using var conn = _factory.Create();
        return await conn.QuerySingleOrDefaultAsync<string>(
            GetAllowedDomainsSql, new { CompanyId = companyId });
    }
}