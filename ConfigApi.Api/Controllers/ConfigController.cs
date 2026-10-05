using System.Security.Claims;
using ConfigApi.Api.Filters;
using ConfigApi.Business.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ConfigApi.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[DomainCheck]
public class ConfigController : ControllerBase
{
    private readonly IConfigService _configService;

    public ConfigController(IConfigService configService)
        => _configService = configService;

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var companyId = int.Parse(User.FindFirst("companyId")!.Value);
        var companyName = User.FindFirst("companyName")?.Value ?? "";

        var result = await _configService.GetByCompanyAsync(companyId, companyName);
        return Ok(result);
    }
}