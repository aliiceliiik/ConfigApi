using ConfigApi.Business.Services;
using ConfigApi.Entities.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ConfigApi.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProductsController : ControllerBase
{
    private readonly IProductService _products;
<<<<<<< HEAD

=======
>>>>>>> bcdf37d9dda12d4a7aca61ef0d9fa06bf7e79f43
    public ProductsController(IProductService products) => _products = products;

    [HttpGet]
    public async Task<IActionResult> Search([FromQuery] ProductSearchRequest request)
        => Ok(await _products.SearchAsync(request));

<<<<<<< HEAD
    [HttpGet("suggest")]
    public async Task<IActionResult> Suggest([FromQuery] string q)
        => Ok(await _products.SuggestAsync(q));

=======
>>>>>>> bcdf37d9dda12d4a7aca61ef0d9fa06bf7e79f43
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var product = await _products.GetByIdAsync(id);
        return product is null
            ? NotFound(new { message = "Ürün bulunamadı." })
            : Ok(product);
    }
}
