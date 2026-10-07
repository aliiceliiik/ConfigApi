using ConfigApi.Entities.Dtos;
using ConfigApi.Entities.Enums;
using ConfigApi.Mvc.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ConfigApi.Mvc.Controllers;

[Authorize(Roles = UserRole.SuperAdmin)]
public class TenantsController : Controller
{
    private readonly IApiClient _api;
    private readonly ITenantSelection _selection;

    public TenantsController(IApiClient api, ITenantSelection selection)
    {
        _api = api;
        _selection = selection;
    }

    public async Task<IActionResult> Index()
    {
        var tenants = await _api.GetAsync<List<TenantListItemDto>>("/api/tenants");
        return View(tenants ?? []);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Select(Guid id)
    {
        var tenants = await _api.GetAsync<List<TenantListItemDto>>("/api/tenants");
        var tenant = tenants?.FirstOrDefault(t => t.Id == id);

        if (tenant is null)
        {
            TempData["Message"] = "Tenant bulunamadı.";
            TempData["MessageType"] = "danger";
            return RedirectToAction(nameof(Index));
        }

        _selection.Select(tenant.Id, tenant.Name);
        return RedirectToAction("Orders", "Admin");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Clear()
    {
        _selection.Clear();
        return RedirectToAction(nameof(Index));
    }
}
