using ConfigApi.Context.Factory;
using ConfigApi.Entities.Dtos;
using ConfigApi.Entities.Entities;
using Dapper;

namespace ConfigApi.Context.Repositories;

public interface IUserRepository
{
    Task<User?> GetForLoginAsync(Guid? tenantId, string email);
    Task<User?> GetByIdAsync(Guid id);
    Task<IEnumerable<UserListItemDto>> GetListForTenantAsync(Guid tenantId);
}

public class UserRepository : IUserRepository
{
    private readonly IDbConnectionFactory _factory;

    public UserRepository(IDbConnectionFactory factory) => _factory = factory;

    private const string Columns =
        "Id, TenantId, Email, PasswordHash, FullName, Role, IsActive, CreatedAt";

    private const string GetForLoginSql = $@"
        SELECT  {Columns}
        FROM    Users
        WHERE   Email = @Email
          AND   IsActive = 1
          AND   ((@TenantId IS NULL AND TenantId IS NULL)
              OR (@TenantId IS NOT NULL AND TenantId = @TenantId));";

    public async Task<User?> GetForLoginAsync(Guid? tenantId, string email)
    {
        using var conn = _factory.Create();
        return await conn.QuerySingleOrDefaultAsync<User>(
            GetForLoginSql, new { TenantId = tenantId, Email = email });
    }

    public async Task<User?> GetByIdAsync(Guid id)
    {
        using var conn = _factory.Create();
        return await conn.QuerySingleOrDefaultAsync<User>(
            $"SELECT {Columns} FROM Users WHERE Id = @Id AND IsActive = 1;", new { Id = id });
    }

    private const string GetListForTenantSql = @"
        SELECT  u.Id, u.Email, u.FullName, u.Role, u.IsActive, u.CreatedAt,
                (SELECT COUNT(*) FROM Orders o
                 WHERE o.UserId = u.Id AND o.TenantId = @TenantId) AS OrderCount
        FROM    Users u
        WHERE   u.TenantId = @TenantId
        ORDER BY u.CreatedAt DESC;";

    public async Task<IEnumerable<UserListItemDto>> GetListForTenantAsync(Guid tenantId)
    {
        using var conn = _factory.Create();
        return await conn.QueryAsync<UserListItemDto>(
            GetListForTenantSql, new { TenantId = tenantId });
    }
}
