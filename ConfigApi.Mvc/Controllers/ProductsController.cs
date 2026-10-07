<<<<<<< HEAD
﻿using System.Text;
using ConfigApi.Entities.Dtos;
=======
﻿using ConfigApi.Entities.Dtos;
>>>>>>> bcdf37d9dda12d4a7aca61ef0d9fa06bf7e79f43
using ConfigApi.Mvc.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ConfigApi.Mvc.Controllers;

[Authorize]
public class ProductsController : Controller
{
    private readonly IApiClient _api;

    public ProductsController(IApiClient api) => _api = api;

<<<<<<< HEAD
    public async Task<IActionResult> Index([FromQuery] ProductSearchRequest request)
    {
        if (request.Page < 1) request.Page = 1;
        if (request.PageSize < 1) request.PageSize = 12;

        var query = new StringBuilder("/api/products?page=")
            .Append(request.Page)
            .Append("&pageSize=")
            .Append(request.PageSize);

        if (!string.IsNullOrWhiteSpace(request.Search))
            query.Append("&search=").Append(Uri.EscapeDataString(request.Search));

        if (request.MinPrice is not null)
            query.Append("&minPrice=").Append(request.MinPrice.Value.ToString("0.##"));

        if (request.MaxPrice is not null)
            query.Append("&maxPrice=").Append(request.MaxPrice.Value.ToString("0.##"));

        if (request.InStockOnly)
            query.Append("&inStockOnly=true");

        if (request.Sort != ProductSort.Relevance)
            query.Append("&sort=").Append((int)request.Sort);

        var result = await _api.GetAsync<PagedResult<ProductListItemDto>>(query.ToString());

        ViewBag.Filter = request;
        return View(result ?? new PagedResult<ProductListItemDto>());
    }

    [HttpGet]
    public async Task<IActionResult> Suggest(string q)
    {
        if (string.IsNullOrWhiteSpace(q) || q.Trim().Length < 2)
            return Json(Array.Empty<string>());

        var result = await _api.GetAsync<List<string>>(
            $"/api/products/suggest?q={Uri.EscapeDataString(q.Trim())}");

        return Json(result ?? new List<string>());
    }
=======
    public async Task<IActionResult> Index(string? search, int page = 1)
    {
        var query = $"/api/products?page={page}&pageSize=12";
        if (!string.IsNullOrWhiteSpace(search))
            query += $"&search={Uri.EscapeDataString(search)}";

        var result = await _api.GetAsync<PagedResult<ProductListItemDto>>(query);

        ViewBag.Search = search;
        return View(result ?? new PagedResult<ProductListItemDto>());
    }
>>>>>>> bcdf37d9dda12d4a7aca61ef0d9fa06bf7e79f43
}
