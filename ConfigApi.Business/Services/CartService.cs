using ConfigApi.Business.Tenancy;
<<<<<<< HEAD
using ConfigApi.Context.Caching;
=======
>>>>>>> bcdf37d9dda12d4a7aca61ef0d9fa06bf7e79f43
using ConfigApi.Context.Repositories;
using ConfigApi.Entities.Dtos;

namespace ConfigApi.Business.Services;

public enum CartError { None, ProductNotFound, InsufficientStock, ItemNotFound, InvalidQuantity }

public record CartResult(bool Success, CartError Error = CartError.None, string? Message = null)
{
    public static CartResult Ok() => new(true);
    public static CartResult Fail(CartError error, string message) => new(false, error, message);
}

public interface ICartService
{
    Task<CartDto> GetAsync();
    Task<CartResult> AddAsync(AddToCartRequest request);
    Task<CartResult> UpdateQuantityAsync(Guid itemId, int quantity);
    Task<CartResult> RemoveAsync(Guid itemId);
    Task ClearAsync();
}

public class CartService : ICartService
{
    private const int MaxQuantityPerItem = 100;
<<<<<<< HEAD
    private static readonly TimeSpan Ttl = TimeSpan.FromMinutes(10);

    private readonly ICartRepository _carts;
    private readonly IProductRepository _products;
    private readonly ICacheService _cache;
    private readonly ITenantContext _tenant;

    public CartService(ICartRepository carts, IProductRepository products,
                       ICacheService cache, ITenantContext tenant)
    {
        _carts = carts;
        _products = products;
        _cache = cache;
=======

    private readonly ICartRepository _carts;
    private readonly IProductRepository _products;
    private readonly ITenantContext _tenant;

    public CartService(ICartRepository carts, IProductRepository products, ITenantContext tenant)
    {
        _carts = carts;
        _products = products;
>>>>>>> bcdf37d9dda12d4a7aca61ef0d9fa06bf7e79f43
        _tenant = tenant;
    }

    public async Task<CartDto> GetAsync()
    {
<<<<<<< HEAD
        var key = CacheKeys.Cart(_tenant.TenantId, _tenant.UserId);

        return await _cache.GetOrSetAsync(key, Ttl, async () =>
        {
            var cart = await _carts.GetOrCreateForCurrentUserAsync(_tenant.UserId);
            var items = await _carts.GetItemsAsync(cart.Id);

            return new CartDto { Id = cart.Id, Items = items.ToList() };
        });
=======
        var cart = await _carts.GetOrCreateForCurrentUserAsync(_tenant.UserId);
        var items = await _carts.GetItemsAsync(cart.Id);

        return new CartDto { Id = cart.Id, Items = items.ToList() };
>>>>>>> bcdf37d9dda12d4a7aca61ef0d9fa06bf7e79f43
    }

    public async Task<CartResult> AddAsync(AddToCartRequest request)
    {
        if (request.Quantity < 1)
            return CartResult.Fail(CartError.InvalidQuantity, "Miktar en az 1 olmalı.");

        var product = await _products.GetByIdAsync(request.ProductId);
        if (product is null)
            return CartResult.Fail(CartError.ProductNotFound, "Ürün bulunamadı.");

        var cart = await _carts.GetOrCreateForCurrentUserAsync(_tenant.UserId);
        var existing = await _carts.GetItemByProductAsync(cart.Id, product.Id);

        var targetQuantity = (existing?.Quantity ?? 0) + request.Quantity;

        if (targetQuantity > MaxQuantityPerItem)
            return CartResult.Fail(CartError.InvalidQuantity,
                $"Bir üründen en fazla {MaxQuantityPerItem} adet alabilirsiniz.");

        if (targetQuantity > product.Stock)
            return CartResult.Fail(CartError.InsufficientStock,
                $"Yeterli stok yok. Mevcut: {product.Stock} adet.");

        if (existing is null)
            await _carts.AddItemAsync(cart.Id, product.Id, targetQuantity);
        else
            await _carts.UpdateItemQuantityAsync(existing.Id, targetQuantity);

<<<<<<< HEAD
        await InvalidateAsync();
=======
>>>>>>> bcdf37d9dda12d4a7aca61ef0d9fa06bf7e79f43
        return CartResult.Ok();
    }

    public async Task<CartResult> UpdateQuantityAsync(Guid itemId, int quantity)
    {
        if (quantity < 1 || quantity > MaxQuantityPerItem)
            return CartResult.Fail(CartError.InvalidQuantity, "Geçersiz miktar.");

        var cart = await _carts.GetOrCreateForCurrentUserAsync(_tenant.UserId);

        var item = await _carts.GetItemByIdAsync(cart.Id, itemId);
        if (item is null)
            return CartResult.Fail(CartError.ItemNotFound, "Sepet satırı bulunamadı.");

        var product = await _products.GetByIdAsync(item.ProductId);
        if (product is null)
            return CartResult.Fail(CartError.ProductNotFound, "Ürün artık mevcut değil.");

        if (quantity > product.Stock)
            return CartResult.Fail(CartError.InsufficientStock,
                $"Yeterli stok yok. Mevcut: {product.Stock} adet.");

        await _carts.UpdateItemQuantityAsync(item.Id, quantity);
<<<<<<< HEAD

        await InvalidateAsync();
=======
>>>>>>> bcdf37d9dda12d4a7aca61ef0d9fa06bf7e79f43
        return CartResult.Ok();
    }

    public async Task<CartResult> RemoveAsync(Guid itemId)
    {
        var cart = await _carts.GetOrCreateForCurrentUserAsync(_tenant.UserId);

        var item = await _carts.GetItemByIdAsync(cart.Id, itemId);
        if (item is null)
            return CartResult.Fail(CartError.ItemNotFound, "Sepet satırı bulunamadı.");

        await _carts.RemoveItemAsync(item.Id);
<<<<<<< HEAD

        await InvalidateAsync();
=======
>>>>>>> bcdf37d9dda12d4a7aca61ef0d9fa06bf7e79f43
        return CartResult.Ok();
    }

    public async Task ClearAsync()
    {
        var cart = await _carts.GetOrCreateForCurrentUserAsync(_tenant.UserId);
        await _carts.ClearAsync(cart.Id);
<<<<<<< HEAD

        await InvalidateAsync();
    }

    private Task InvalidateAsync()
        => _cache.RemoveAsync(CacheKeys.Cart(_tenant.TenantId, _tenant.UserId));
=======
    }
>>>>>>> bcdf37d9dda12d4a7aca61ef0d9fa06bf7e79f43
}
