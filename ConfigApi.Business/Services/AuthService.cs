using ConfigApi.Business.Security;
using ConfigApi.Context.Repositories;
using ConfigApi.Entities.Dtos;

namespace ConfigApi.Business.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly ITokenService _tokenService;

    public AuthService(IUserRepository userRepository, ITokenService tokenService)
    {
        _userRepository = userRepository;
        _tokenService = tokenService;
    }

    public async Task<LoginResponse?> LoginAsync(LoginRequest request, string? origin)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email);
        if (user is null)
            return null;

        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            return null;

        var domain = DomainHelper.Extract(origin);
        if (!DomainHelper.IsAllowed(domain, user.AllowedDomains))
            return null;

        var (token, expiresAt) = _tokenService.Create(user, domain ?? "");

        return new LoginResponse
        {
            Token = token,
            ExpiresAt = expiresAt,
            CompanyName = user.CompanyName
        };
    }
}