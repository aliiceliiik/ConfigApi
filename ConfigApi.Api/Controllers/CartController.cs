using ConfigApi.Business.Services;
using ConfigApi.Entities.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ConfigApi.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CartController : ControllerBase
{
    private readonly ICartService _cart;
    public CartController(ICartService cart) => _cart = cart;

    [HttpGet]
    public async Task<IActionResult> Get() => Ok(await _cart.GetAsync());

    [HttpPost("items")]
    public async Task<IActionResult> Add([FromBody] AddToCartRequest request)
    {
        var result = await _cart.AddAsync(request);
        return result.Success ? Ok(await _cart.GetAsync()) : ToError(result);
    }

    [HttpPut("items/{itemId:guid}")]
    public async Task<IActionResult> Update(Guid itemId, [FromBody] UpdateCartItemRequest request)
    {
        var result = await _cart.UpdateQuantityAsync(itemId, request.Quantity);
        return result.Success ? Ok(await _cart.GetAsync()) : ToError(result);
    }

    [HttpDelete("items/{itemId:guid}")]
    public async Task<IActionResult> Remove(Guid itemId)
    {
        var result = await _cart.RemoveAsync(itemId);
        return result.Success ? Ok(await _cart.GetAsync()) : ToError(result);
    }

    [HttpDelete]
    public async Task<IActionResult> Clear()
    {
        await _cart.ClearAsync();
        return NoContent();
    }

    private IActionResult ToError(CartResult result) => result.Error switch
    {
        CartError.ProductNotFound or CartError.ItemNotFound
            => NotFound(new { message = result.Message }),
        _ => BadRequest(new { message = result.Message })
    };
}
