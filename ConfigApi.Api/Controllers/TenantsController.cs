using ConfigApi.Business.Services;
using ConfigApi.Entities.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ConfigApi.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = UserRole.SuperAdmin)]
public class TenantsController : ControllerBase
{
    private readonly IAdminService _admin;

    public TenantsController(IAdminService admin) => _admin = admin;

    [HttpGet]
    public async Task<IActionResult> GetAll()
        => Ok(await _admin.GetTenantsAsync());
}
