using ConfigApi.Context.Factory;
using Dapper;

namespace ConfigApi.Context.Repositories;

public interface IRefreshTokenRepository
{
    Task CreateAsync(Guid userId, string tokenHash, DateTime expiresAt);
    Task<Guid?> GetValidUserIdAsync(string tokenHash);
    Task RevokeAsync(string tokenHash);
    Task RevokeAllForUserAsync(Guid userId);
}

public class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly IDbConnectionFactory _factory;

    public RefreshTokenRepository(IDbConnectionFactory factory) => _factory = factory;

    public async Task CreateAsync(Guid userId, string tokenHash, DateTime expiresAt)
    {
        using var conn = _factory.Create();
        await conn.ExecuteAsync(@"
            INSERT INTO RefreshTokens (UserId, TokenHash, ExpiresAt)
            VALUES (@UserId, @TokenHash, @ExpiresAt);",
            new { UserId = userId, TokenHash = tokenHash, ExpiresAt = expiresAt });
    }

    public async Task<Guid?> GetValidUserIdAsync(string tokenHash)
    {
        using var conn = _factory.Create();
        return await conn.QuerySingleOrDefaultAsync<Guid?>(@"
            SELECT  UserId
            FROM    RefreshTokens
            WHERE   TokenHash = @TokenHash
              AND   RevokedAt IS NULL
              AND   ExpiresAt > SYSUTCDATETIME();",
            new { TokenHash = tokenHash });
    }

    public async Task RevokeAsync(string tokenHash)
    {
        using var conn = _factory.Create();
        await conn.ExecuteAsync(@"
            UPDATE RefreshTokens
            SET    RevokedAt = SYSUTCDATETIME()
            WHERE  TokenHash = @TokenHash AND RevokedAt IS NULL;",
            new { TokenHash = tokenHash });
    }

    public async Task RevokeAllForUserAsync(Guid userId)
    {
        using var conn = _factory.Create();
        await conn.ExecuteAsync(@"
            UPDATE RefreshTokens
            SET    RevokedAt = SYSUTCDATETIME()
            WHERE  UserId = @UserId AND RevokedAt IS NULL;",
            new { UserId = userId });
    }
}
