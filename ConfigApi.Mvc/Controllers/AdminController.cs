using ConfigApi.Entities.Dtos;
using ConfigApi.Entities.Enums;
using ConfigApi.Mvc.Filters;
using ConfigApi.Mvc.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ConfigApi.Mvc.Controllers;

[Authorize(Roles = UserRole.TenantAdmin + "," + UserRole.SuperAdmin)]
[RequireTenantSelection]
public class AdminController : Controller
{
    private readonly IApiClient _api;

    public AdminController(IApiClient api) => _api = api;

    public async Task<IActionResult> Orders()
    {
        var orders = await _api.GetAsync<List<OrderListItemDto>>("/api/admin/orders");
        return View(orders ?? []);
    }

    public async Task<IActionResult> OrderDetail(Guid id)
    {
        var order = await _api.GetAsync<OrderDto>($"/api/admin/orders/{id}");
        if (order is null) return NotFound();

        return View(order);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(Guid id, byte status)
    {
        var result = await _api.PutAsync<object>($"/api/admin/orders/{id}/status",
            new UpdateOrderStatusRequest { Status = status });

        TempData["Message"] = result.Success ? "Durum güncellendi." : result.Message;
        TempData["MessageType"] = result.Success ? "success" : "danger";

        return RedirectToAction(nameof(OrderDetail), new { id });
    }

    public async Task<IActionResult> Users()
    {
        var users = await _api.GetAsync<List<UserListItemDto>>("/api/admin/users");
        return View(users ?? []);
    }

    public async Task<IActionResult> Products()
    {
        var products = await _api.GetAsync<List<ProductAdminDto>>("/api/admin/products");
        return View(products ?? []);
    }

    [HttpGet]
    public IActionResult CreateProduct()
    {
        ViewBag.ProductId = null;
        return View("ProductForm", new ProductSaveRequest());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateProduct(ProductSaveRequest model)
    {
        ViewBag.ProductId = null;

        if (!ModelState.IsValid) return View("ProductForm", model);

        var result = await _api.PostAsync<object>("/api/admin/products", model);

        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.Message ?? "Kaydedilemedi.");
            return View("ProductForm", model);
        }

        TempData["Message"] = "Ürün eklendi.";
        TempData["MessageType"] = "success";

        return RedirectToAction(nameof(Products));
    }

    [HttpGet]
    public async Task<IActionResult> EditProduct(Guid id)
    {
        var product = await _api.GetAsync<ProductAdminDto>($"/api/admin/products/{id}");
        if (product is null) return NotFound();

        ViewBag.ProductId = id;

        return View("ProductForm", new ProductSaveRequest
        {
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            Stock = product.Stock,
            IsActive = product.IsActive
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditProduct(Guid id, ProductSaveRequest model)
    {
        ViewBag.ProductId = id;

        if (!ModelState.IsValid) return View("ProductForm", model);

        var result = await _api.PutAsync<object>($"/api/admin/products/{id}", model);

        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.Message ?? "Kaydedilemedi.");
            return View("ProductForm", model);
        }

        TempData["Message"] = "Ürün güncellendi.";
        TempData["MessageType"] = "success";

        return RedirectToAction(nameof(Products));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleProduct(Guid id, bool isActive)
    {
        var target = (!isActive).ToString().ToLowerInvariant();
        var result = await _api.PatchAsync($"/api/admin/products/{id}/active?value={target}");

        TempData["Message"] = result.Success ? "Ürün durumu güncellendi." : result.Message;
        TempData["MessageType"] = result.Success ? "success" : "danger";

        return RedirectToAction(nameof(Products));
    }
}
