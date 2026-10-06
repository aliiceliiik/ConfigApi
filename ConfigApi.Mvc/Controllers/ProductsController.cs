using ConfigApi.Entities.Dtos;
using ConfigApi.Mvc.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ConfigApi.Mvc.Controllers;

[Authorize]
public class ProductsController : Controller
{
    private readonly IApiClient _api;

    public ProductsController(IApiClient api) => _api = api;

    public async Task<IActionResult> Index(string? search, int page = 1)
    {
        var query = $"/api/products?page={page}&pageSize=12";
        if (!string.IsNullOrWhiteSpace(search))
            query += $"&search={Uri.EscapeDataString(search)}";

        var result = await _api.GetAsync<PagedResult<ProductListItemDto>>(query);

        ViewBag.Search = search;
        return View(result ?? new PagedResult<ProductListItemDto>());
    }
}
