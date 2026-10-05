using ConfigApi.Entities.Dtos;

namespace ConfigApi.Business.Services;

public interface IAuthService
{
    Task<LoginResponse?> LoginAsync(LoginRequest request, string? origin);
}