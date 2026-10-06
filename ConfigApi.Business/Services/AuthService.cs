using ConfigApi.Business.Security;
using ConfigApi.Context.Repositories;
using ConfigApi.Entities.Dtos;
using ConfigApi.Entities.Entities;
using Microsoft.Extensions.Configuration;

namespace ConfigApi.Business.Services;

public interface IAuthService
{
    Task<TokenResponse?> LoginAsync(Guid? tenantId, LoginRequest request);
    Task<TokenResponse?> RefreshAsync(string refreshToken);
    Task LogoutAsync(string refreshToken);
}

public class AuthService : IAuthService
{
    private readonly IUserRepository _users;
    private readonly ITenantRepository _tenants;
    private readonly IRefreshTokenRepository _refreshTokens;
    private readonly ITokenService _tokens;
    private readonly IConfiguration _config;

    public AuthService(IUserRepository users, ITenantRepository tenants,
                       IRefreshTokenRepository refreshTokens,
                       ITokenService tokens, IConfiguration config)
    {
        _users = users;
        _tenants = tenants;
        _refreshTokens = refreshTokens;
        _tokens = tokens;
        _config = config;
    }

    public async Task<TokenResponse?> LoginAsync(Guid? tenantId, LoginRequest request)
    {
        var user = await _users.GetForLoginAsync(tenantId, request.Email);

        if (user is null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            return null;

        return await IssueAsync(user);
    }

    public async Task<TokenResponse?> RefreshAsync(string refreshToken)
    {
        if (string.IsNullOrWhiteSpace(refreshToken)) return null;

        var hash = _tokens.Hash(refreshToken);
        var userId = await _refreshTokens.GetValidUserIdAsync(hash);
        if (userId is null) return null;

        var user = await _users.GetByIdAsync(userId.Value);
        if (user is null) return null;

        await _refreshTokens.RevokeAsync(hash);
        return await IssueAsync(user);
    }

    public async Task LogoutAsync(string refreshToken)
    {
        if (string.IsNullOrWhiteSpace(refreshToken)) return;
        await _refreshTokens.RevokeAsync(_tokens.Hash(refreshToken));
    }

    private async Task<TokenResponse> IssueAsync(User user)
    {
        string? tenantName = null;
        if (user.TenantId is not null)
            tenantName = (await _tenants.GetByIdAsync(user.TenantId.Value))?.Name;

        var (accessToken, expiresAt) = _tokens.CreateAccessToken(user, tenantName);
        var refreshToken = _tokens.CreateRefreshToken();
        var refreshDays = int.Parse(_config["Jwt:RefreshTokenDays"]!);

        await _refreshTokens.CreateAsync(
            user.Id, _tokens.Hash(refreshToken), DateTime.UtcNow.AddDays(refreshDays));

        return new TokenResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresAt = expiresAt,
            FullName = user.FullName,
            Role = user.Role
        };
    }
}
