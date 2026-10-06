using ConfigApi.Context.Repositories;
using ConfigApi.Entities.Dtos;
using ConfigApi.Entities.Entities;

namespace ConfigApi.Business.Services;

public interface IProductService
{
    Task<PagedResult<ProductListItemDto>> SearchAsync(ProductSearchRequest request);
    Task<ProductListItemDto?> GetByIdAsync(Guid id);
}

public class ProductService : IProductService
{
    private const int MaxPageSize = 100;

    private readonly IProductRepository _products;

    public ProductService(IProductRepository products) => _products = products;

    public async Task<PagedResult<ProductListItemDto>> SearchAsync(ProductSearchRequest request)
    {
        var page = request.Page < 1 ? 1 : request.Page;
        var pageSize = request.PageSize < 1 ? 20 : Math.Min(request.PageSize, MaxPageSize);

        var (items, total) = await _products.SearchAsync(request.Search, page, pageSize);

        return new PagedResult<ProductListItemDto>
        {
            Items = items.Select(Map).ToList(),
            TotalCount = total,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<ProductListItemDto?> GetByIdAsync(Guid id)
    {
        var product = await _products.GetByIdAsync(id);
        return product is null ? null : Map(product);
    }

    private static ProductListItemDto Map(Product p) => new()
    {
        Id = p.Id,
        Name = p.Name,
        Description = p.Description,
        Price = p.Price,
        Stock = p.Stock
    };
}
