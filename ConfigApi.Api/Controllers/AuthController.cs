using ConfigApi.Api.Middleware;
using ConfigApi.Business.Services;
using ConfigApi.Entities.Dtos;
using ConfigApi.Entities.Entities;
using Microsoft.AspNetCore.Mvc;

namespace ConfigApi.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService) => _authService = authService;

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var tenant = HttpContext.Items[TenantResolutionMiddleware.TenantItemKey] as Tenant;
        var host = HttpContext.Request.Host.Host.ToLowerInvariant();

        if (tenant is null && !host.StartsWith("admin."))
            return BadRequest(new { message = "Geçersiz adres." });

        var result = await _authService.LoginAsync(tenant?.Id, request);

        return result is null
            ? Unauthorized(new { message = "E-posta veya şifre hatalı." })
            : Ok(result);
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshRequest request)
    {
        var result = await _authService.RefreshAsync(request.RefreshToken);

        return result is null
            ? Unauthorized(new { message = "Oturum süresi doldu, tekrar giriş yapın." })
            : Ok(result);
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] RefreshRequest request)
    {
        await _authService.LogoutAsync(request.RefreshToken);
        return NoContent();
    }
}
