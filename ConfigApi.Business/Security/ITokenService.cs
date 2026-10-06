using ConfigApi.Entities.Entities;

namespace ConfigApi.Business.Security;

public interface ITokenService
{
    (string token, DateTime expiresAt) CreateAccessToken(User user, string? tenantName);
    string CreateRefreshToken();
    string Hash(string token);
}
