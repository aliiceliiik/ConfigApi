using ConfigApi.Context.Factory;
using ConfigApi.Entities.Dtos;
using Dapper;

namespace ConfigApi.Context.Repositories;

public class UserRepository : IUserRepository
{
    private readonly IDbConnectionFactory _factory;

    public UserRepository(IDbConnectionFactory factory) => _factory = factory;

    private const string GetByEmailSql = @"
        SELECT  u.Id, u.Email, u.PasswordHash, u.IsActive,
                c.Id AS CompanyId, c.Name AS CompanyName, c.AllowedDomains
        FROM    Users u
        JOIN    Companies c ON c.Id = u.CompanyId
        WHERE   u.Email = @Email AND u.IsActive = 1 AND c.IsActive = 1;";

    public async Task<UserDto?> GetByEmailAsync(string email)
    {
        using var conn = _factory.Create();
        return await conn.QuerySingleOrDefaultAsync<UserDto>(
            GetByEmailSql, new { Email = email });
    }
}