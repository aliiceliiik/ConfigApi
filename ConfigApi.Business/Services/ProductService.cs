<<<<<<< HEAD
﻿using ConfigApi.Business.Tenancy;
using ConfigApi.Context.Caching;
using ConfigApi.Context.Repositories;
using ConfigApi.Context.Search;
using ConfigApi.Entities.Dtos;
using ConfigApi.Entities.Entities;
using Microsoft.Extensions.Logging;
=======
﻿using ConfigApi.Context.Repositories;
using ConfigApi.Entities.Dtos;
using ConfigApi.Entities.Entities;
>>>>>>> bcdf37d9dda12d4a7aca61ef0d9fa06bf7e79f43

namespace ConfigApi.Business.Services;

public interface IProductService
{
    Task<PagedResult<ProductListItemDto>> SearchAsync(ProductSearchRequest request);
    Task<ProductListItemDto?> GetByIdAsync(Guid id);
<<<<<<< HEAD
    Task<IEnumerable<string>> SuggestAsync(string prefix);
=======
>>>>>>> bcdf37d9dda12d4a7aca61ef0d9fa06bf7e79f43
}

public class ProductService : IProductService
{
    private const int MaxPageSize = 100;
<<<<<<< HEAD
    private const int SuggestSize = 8;
    private static readonly TimeSpan SearchTtl = TimeSpan.FromMinutes(5);

    private readonly IProductRepository _products;
    private readonly IProductSearchQuery _search;
    private readonly ICacheService _cache;
    private readonly ITenantContext _tenant;
    private readonly ILogger<ProductService> _logger;

    public ProductService(IProductRepository products,
                          IProductSearchQuery search,
                          ICacheService cache,
                          ITenantContext tenant,
                          ILogger<ProductService> logger)
    {
        _products = products;
        _search = search;
        _cache = cache;
        _tenant = tenant;
        _logger = logger;
    }

    public async Task<PagedResult<ProductListItemDto>> SearchAsync(ProductSearchRequest request)
    {
        request.Page = request.Page < 1 ? 1 : request.Page;
        request.PageSize = request.PageSize < 1 ? 12 : Math.Min(request.PageSize, MaxPageSize);

        var key = CacheKeys.ProductSearch(_tenant.TenantId, request.CacheSignature());

        return await _cache.GetOrSetAsync(key, SearchTtl, () => ExecuteSearchAsync(request));
    }

    private async Task<PagedResult<ProductListItemDto>> ExecuteSearchAsync(ProductSearchRequest request)
    {
        if (_search.IsEnabled)
        {
            try
            {
                var (items, total) = await _search.SearchAsync(request);

                return new PagedResult<ProductListItemDto>
                {
                    Items = items.ToList(),
                    TotalCount = total,
                    Page = request.Page,
                    PageSize = request.PageSize
                };
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Arama indeksi kullanilamadi, SQL'e dusuluyor.");
            }
        }

        var (sqlItems, sqlTotal) = await _products.SearchAsync(request);

        return new PagedResult<ProductListItemDto>
        {
            Items = sqlItems.Select(Map).ToList(),
            TotalCount = sqlTotal,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }

    public async Task<IEnumerable<string>> SuggestAsync(string prefix)
    {
        if (!_search.IsEnabled || string.IsNullOrWhiteSpace(prefix))
            return Array.Empty<string>();

        try
        {
            return await _search.SuggestAsync(prefix, SuggestSize);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Oneri alinamadi.");
            return Array.Empty<string>();
        }
    }

=======

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

>>>>>>> bcdf37d9dda12d4a7aca61ef0d9fa06bf7e79f43
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
