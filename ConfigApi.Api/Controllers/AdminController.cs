using ConfigApi.Api.Filters;
using ConfigApi.Business.Services;
using ConfigApi.Entities.Dtos;
using ConfigApi.Entities.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ConfigApi.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = UserRole.TenantAdmin + "," + UserRole.SuperAdmin)]
[ResolveAdminScope]
public class AdminController : ControllerBase
{
    private readonly IAdminService _admin;

    public AdminController(IAdminService admin) => _admin = admin;

    [HttpGet("orders")]
    public async Task<IActionResult> GetOrders()
        => Ok(await _admin.GetOrdersAsync());

    [HttpGet("orders/{id:guid}")]
    public async Task<IActionResult> GetOrderDetail(Guid id)
    {
        var order = await _admin.GetOrderDetailAsync(id);
        return order is null
            ? NotFound(new { message = "Sipariş bulunamadı." })
            : Ok(order);
    }

    [HttpPut("orders/{id:guid}/status")]
    public async Task<IActionResult> UpdateOrderStatus(Guid id, [FromBody] UpdateOrderStatusRequest request)
    {
        var updated = await _admin.UpdateOrderStatusAsync(id, request.Status);
        return updated
            ? NoContent()
            : NotFound(new { message = "Sipariş bulunamadı." });
    }

    [HttpGet("users")]
    public async Task<IActionResult> GetUsers()
        => Ok(await _admin.GetUsersAsync());

    [HttpGet("products")]
    public async Task<IActionResult> GetProducts()
        => Ok(await _admin.GetProductsAsync());

    [HttpGet("products/{id:guid}")]
    public async Task<IActionResult> GetProduct(Guid id)
    {
        var product = await _admin.GetProductAsync(id);
        return product is null
            ? NotFound(new { message = "Ürün bulunamadı." })
            : Ok(product);
    }

    [HttpPost("products")]
    public async Task<IActionResult> CreateProduct([FromBody] ProductSaveRequest request)
    {
        var id = await _admin.CreateProductAsync(request);
        return CreatedAtAction(nameof(GetProduct), new { id }, new { id });
    }

    [HttpPut("products/{id:guid}")]
    public async Task<IActionResult> UpdateProduct(Guid id, [FromBody] ProductSaveRequest request)
    {
        var updated = await _admin.UpdateProductAsync(id, request);
        return updated
            ? NoContent()
            : NotFound(new { message = "Ürün bulunamadı." });
    }

    [HttpPatch("products/{id:guid}/active")]
    public async Task<IActionResult> SetProductActive(Guid id, [FromQuery] bool value)
    {
        var updated = await _admin.SetProductActiveAsync(id, value);
        return updated
            ? NoContent()
            : NotFound(new { message = "Ürün bulunamadı." });
    }
<<<<<<< HEAD

    [HttpPost("reindex")]
    [Authorize(Roles = UserRole.SuperAdmin)]
    public async Task<IActionResult> Reindex()
    {
        var count = await _admin.ReindexProductsAsync();
        return Ok(new { indexed = count });
    }
=======
>>>>>>> bcdf37d9dda12d4a7aca61ef0d9fa06bf7e79f43
}
