using ConfigApi.Entities.Dtos;

namespace ConfigApi.Business.Security;

public interface ITokenService
{
    (string token, DateTime expiresAt) Create(UserDto user, string domain);
}