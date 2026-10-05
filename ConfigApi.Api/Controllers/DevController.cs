using Microsoft.AspNetCore.Mvc;

namespace ConfigApi.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DevController : ControllerBase
{
    [HttpGet("hash")]
    public IActionResult Hash([FromQuery] string password)
        => Ok(new { hash = BCrypt.Net.BCrypt.HashPassword(password) });
}