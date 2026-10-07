using ConfigApi.Business.Tenancy;
<<<<<<< HEAD
using ConfigApi.Context.Caching;
using ConfigApi.Context.Repositories;
using ConfigApi.Context.Search;
=======
using ConfigApi.Context.Repositories;
>>>>>>> bcdf37d9dda12d4a7aca61ef0d9fa06bf7e79f43
using ConfigApi.Entities.Dtos;
using ConfigApi.Entities.Entities;
using ConfigApi.Entities.Enums;

namespace ConfigApi.Business.Services;

public interface IAdminService
{
    Task<IEnumerable<OrderListItemDto>> GetOrdersAsync();
    Task<OrderDto?> GetOrderDetailAsync(Guid orderId);
    Task<bool> UpdateOrderStatusAsync(Guid orderId, byte status);
    Task<IEnumerable<UserListItemDto>> GetUsersAsync();
    Task<IEnumerable<TenantListItemDto>> GetTenantsAsync();
    Task<IEnumerable<ProductAdminDto>> GetProductsAsync();
    Task<ProductAdminDto?> GetProductAsync(Guid id);
    Task<Guid> CreateProductAsync(ProductSaveRequest request);
    Task<bool> UpdateProductAsync(Guid id, ProductSaveRequest request);
    Task<bool> SetProductActiveAsync(Guid id, bool isActive);
<<<<<<< HEAD
    Task<int> ReindexProductsAsync();
=======
>>>>>>> bcdf37d9dda12d4a7aca61ef0d9fa06bf7e79f43
}

public class AdminService : IAdminService
{
    private readonly IOrderRepository _orders;
    private readonly IUserRepository _users;
    private readonly ITenantRepository _tenants;
    private readonly IProductRepository _products;
<<<<<<< HEAD
    private readonly IProductSyncService _sync;
    private readonly IProductSearchIndex _index;
    private readonly ICacheService _cache;
=======
>>>>>>> bcdf37d9dda12d4a7aca61ef0d9fa06bf7e79f43
    private readonly ITenantContext _tenant;

    public AdminService(IOrderRepository orders, IUserRepository users,
                        ITenantRepository tenants, IProductRepository products,
<<<<<<< HEAD
                        IProductSyncService sync, IProductSearchIndex index,
                        ICacheService cache, ITenantContext tenant)
=======
                        ITenantContext tenant)
>>>>>>> bcdf37d9dda12d4a7aca61ef0d9fa06bf7e79f43
    {
        _orders = orders;
        _users = users;
        _tenants = tenants;
        _products = products;
<<<<<<< HEAD
        _sync = sync;
        _index = index;
        _cache = cache;
=======
>>>>>>> bcdf37d9dda12d4a7aca61ef0d9fa06bf7e79f43
        _tenant = tenant;
    }

    public async Task<IEnumerable<OrderListItemDto>> GetOrdersAsync()
    {
        var orders = (await _orders.GetForTenantAsync()).ToList();
        foreach (var order in orders)
            order.Status = OrderService.StatusText(order.Status);

        return orders;
    }

    public async Task<OrderDto?> GetOrderDetailAsync(Guid orderId)
    {
        var order = await _orders.GetDetailAsync(orderId, null);
        if (order is not null)
            order.Status = OrderService.StatusText(order.Status);

        return order;
    }

    public async Task<bool> UpdateOrderStatusAsync(Guid orderId, byte status)
    {
        if (!Enum.IsDefined(typeof(OrderStatus), status))
            throw new InvalidOperationException("Geçersiz sipariş durumu.");

        return await _orders.UpdateStatusAsync(orderId, status);
    }

    public async Task<IEnumerable<UserListItemDto>> GetUsersAsync()
        => await _users.GetListForTenantAsync(_tenant.TenantId);

    public async Task<IEnumerable<TenantListItemDto>> GetTenantsAsync()
    {
        if (!_tenant.IsSuperAdmin)
            throw new UnauthorizedAccessException("Bu listeye erişim yetkiniz yok.");

        return await _tenants.GetListAsync();
    }

    public async Task<IEnumerable<ProductAdminDto>> GetProductsAsync()
        => (await _products.GetAllForAdminAsync()).Select(Map).ToList();

    public async Task<ProductAdminDto?> GetProductAsync(Guid id)
    {
        var product = await _products.GetByIdForAdminAsync(id);
        return product is null ? null : Map(product);
    }

    public async Task<Guid> CreateProductAsync(ProductSaveRequest request)
<<<<<<< HEAD
    {
        var product = await _products.CreateAsync(request);
        await _sync.OnProductSavedAsync(product);

        return product.Id;
    }

    public async Task<bool> UpdateProductAsync(Guid id, ProductSaveRequest request)
    {
        var product = await _products.UpdateAsync(id, request);
        if (product is null) return false;

        await _sync.OnProductSavedAsync(product);
        return true;
    }

    public async Task<bool> SetProductActiveAsync(Guid id, bool isActive)
    {
        var product = await _products.SetActiveAsync(id, isActive);
        if (product is null) return false;

        await _sync.OnProductSavedAsync(product);
        return true;
    }

    public async Task<int> ReindexProductsAsync()
    {
        if (!_tenant.IsSuperAdmin)
            throw new UnauthorizedAccessException("Yeniden indeksleme yetkiniz yok.");

        var all = await _products.GetAllForIndexingAsync();
        var count = await _index.ReindexAllAsync(all);

        await _cache.RemoveByPrefixAsync("t:");

        return count;
    }
=======
        => await _products.CreateAsync(request);

    public async Task<bool> UpdateProductAsync(Guid id, ProductSaveRequest request)
        => await _products.UpdateAsync(id, request);

    public async Task<bool> SetProductActiveAsync(Guid id, bool isActive)
        => await _products.SetActiveAsync(id, isActive);
>>>>>>> bcdf37d9dda12d4a7aca61ef0d9fa06bf7e79f43

    private static ProductAdminDto Map(Product p) => new()
    {
        Id = p.Id,
        Name = p.Name,
        Description = p.Description,
        Price = p.Price,
        Stock = p.Stock,
        IsActive = p.IsActive,
        CreatedAt = p.CreatedAt
    };
}
