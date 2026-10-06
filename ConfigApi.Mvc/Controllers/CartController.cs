using ConfigApi.Entities.Dtos;
using ConfigApi.Mvc.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ConfigApi.Mvc.Controllers;

[Authorize]
public class CartController : Controller
{
    private readonly IApiClient _api;

    public CartController(IApiClient api) => _api = api;

    public async Task<IActionResult> Index()
    {
        var cart = await _api.GetAsync<CartDto>("/api/cart");
        return View(cart ?? new CartDto());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Add(Guid productId, int quantity)
    {
        var result = await _api.PostAsync<CartDto>("/api/cart/items",
            new AddToCartRequest { ProductId = productId, Quantity = quantity });

        SetMessage(result.Success, result.Success ? "Ürün sepete eklendi." : result.Message);
        return RedirectToAction("Index", "Products");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(Guid itemId, int quantity)
    {
        var result = await _api.PutAsync<CartDto>($"/api/cart/items/{itemId}",
            new UpdateCartItemRequest { Quantity = quantity });

        SetMessage(result.Success, result.Success ? "Sepet güncellendi." : result.Message);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Remove(Guid itemId)
    {
        var result = await _api.DeleteAsync($"/api/cart/items/{itemId}");

        SetMessage(result.Success, result.Success ? "Ürün sepetten çıkarıldı." : result.Message);
        return RedirectToAction(nameof(Index));
    }

    private void SetMessage(bool success, string? message)
    {
        TempData["Message"] = message;
        TempData["MessageType"] = success ? "success" : "danger";
    }
}
