using ConfigApi.Business.Tenancy;
using ConfigApi.Context.Caching;
using ConfigApi.Context.Repositories;
using ConfigApi.Context.Search;
using ConfigApi.Entities.Entities;

namespace ConfigApi.Business.Services;

public interface IProductSyncService
{
    Task OnProductSavedAsync(Product product);
    Task OnProductsChangedAsync(IEnumerable<Guid> productIds);
    Task InvalidateSearchCacheAsync();
}

public class ProductSyncService : IProductSyncService
{
    private readonly IProductSearchIndex _index;
    private readonly IProductRepository _products;
    private readonly ICacheService _cache;
    private readonly ITenantContext _tenant;

    public ProductSyncService(IProductSearchIndex index,
                              IProductRepository products,
                              ICacheService cache,
                              ITenantContext tenant)
    {
        _index = index;
        _products = products;
        _cache = cache;
        _tenant = tenant;
    }

    public async Task OnProductSavedAsync(Product product)
    {
        await _index.IndexAsync(product);
        await InvalidateSearchCacheAsync();
    }

    public async Task OnProductsChangedAsync(IEnumerable<Guid> productIds)
    {
        var ids = productIds.Distinct().ToList();
        if (ids.Count == 0) return;

        var products = new List<Product>();

        foreach (var id in ids)
        {
            var product = await _products.GetByIdForAdminAsync(id);
            if (product is not null) products.Add(product);
        }

        await _index.IndexManyAsync(products);
        await InvalidateSearchCacheAsync();
    }

    public Task InvalidateSearchCacheAsync()
        => _cache.RemoveByPrefixAsync(CacheKeys.ProductSearchPrefix(_tenant.TenantId));
}
