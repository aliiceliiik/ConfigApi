using ConfigApi.Business.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ConfigApi.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orders;
    public OrdersController(IOrderService orders) => _orders = orders;

    [HttpPost("checkout")]
    public async Task<IActionResult> Checkout()
    {
        var (success, message, order) = await _orders.CheckoutAsync();
        return success
            ? CreatedAtAction(nameof(GetDetail), new { id = order!.Id }, order)
            : BadRequest(new { message });
    }

    [HttpGet]
    public async Task<IActionResult> GetMyOrders()
        => Ok(await _orders.GetMyOrdersAsync());

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetDetail(Guid id)
    {
        var order = await _orders.GetMyOrderDetailAsync(id);
        return order is null
            ? NotFound(new { message = "Sipariş bulunamadı." })
            : Ok(order);
    }
}
