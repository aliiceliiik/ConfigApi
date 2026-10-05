using ConfigApi.Business.Services;
using ConfigApi.Entities.Dtos;
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
        var origin = Request.Headers.Origin.FirstOrDefault()
                     ?? Request.Headers.Referer.FirstOrDefault();

        var result = await _authService.LoginAsync(request, origin);

        if (result is null)
            return Unauthorized(new { message = "E-posta, şifre veya domain geçersiz." });

        return Ok(result);
    }
}