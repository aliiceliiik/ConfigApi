using ConfigApi.Entities.Dtos;
using ConfigApi.Mvc.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ConfigApi.Mvc.Controllers;

[Authorize]
public class OrdersController : Controller
{
    private readonly IApiClient _api;

    public OrdersController(IApiClient api) => _api = api;

    public async Task<IActionResult> Index()
    {
        var orders = await _api.GetAsync<List<OrderListItemDto>>("/api/orders");
        return View(orders ?? []);
    }

    public async Task<IActionResult> Detail(Guid id)
    {
        var order = await _api.GetAsync<OrderDto>($"/api/orders/{id}");
        if (order is null) return NotFound();

        return View(order);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Checkout()
    {
        var result = await _api.PostAsync<OrderDto>("/api/orders/checkout");

        if (!result.Success || result.Data is null)
        {
            TempData["Message"] = result.Message ?? "Sipariş oluşturulamadı.";
            TempData["MessageType"] = "danger";
            return RedirectToAction("Index", "Cart");
        }

        TempData["Message"] = $"Siparişiniz alındı. Sipariş no: {result.Data.OrderNumber}";
        TempData["MessageType"] = "success";

        return RedirectToAction(nameof(Detail), new { id = result.Data.Id });
    }
}
