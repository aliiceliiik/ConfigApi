using ConfigApi.Context.Factory;
using ConfigApi.Context.Repositories.Base;
using ConfigApi.Entities.Dtos;
using ConfigApi.Entities.Entities;
using Dapper;

namespace ConfigApi.Context.Repositories;

public interface ICartRepository
{
    Task<Cart> GetOrCreateForCurrentUserAsync(Guid userId);
    Task<IEnumerable<CartItemDto>> GetItemsAsync(Guid cartId);
    Task<CartItem?> GetItemByProductAsync(Guid cartId, Guid productId);
    Task<CartItem?> GetItemByIdAsync(Guid cartId, Guid itemId);
    Task AddItemAsync(Guid cartId, Guid productId, int quantity);
    Task UpdateItemQuantityAsync(Guid itemId, int quantity);
    Task RemoveItemAsync(Guid itemId);
    Task ClearAsync(Guid cartId);
}

public class CartRepository : TenantScopedRepository, ICartRepository
{
    public CartRepository(IDbConnectionFactory f, ITenantProvider t) : base(f, t) { }

    public async Task<Cart> GetOrCreateForCurrentUserAsync(Guid userId)
    {
        using var conn = Factory.Create();

        var existing = await conn.QuerySingleOrDefaultAsync<Cart>(@"
            SELECT Id, TenantId, UserId, CreatedAt, UpdatedAt
            FROM   Carts
            WHERE  UserId = @UserId AND TenantId = @TenantId;",
            new { UserId = userId, TenantId });

        if (existing is not null) return existing;

        return await conn.QuerySingleAsync<Cart>(@"
            INSERT INTO Carts (TenantId, UserId)
            OUTPUT INSERTED.Id, INSERTED.TenantId, INSERTED.UserId,
                   INSERTED.CreatedAt, INSERTED.UpdatedAt
            VALUES (@TenantId, @UserId);",
            new { TenantId, UserId = userId });
    }

    private const string GetItemsSql = @"
        SELECT  ci.Id, ci.ProductId, p.Name AS ProductName,
                p.Price AS UnitPrice, ci.Quantity, p.Stock AS AvailableStock
        FROM    CartItems ci
        JOIN    Carts c    ON c.Id = ci.CartId
        JOIN    Products p ON p.Id = ci.ProductId
        WHERE   ci.CartId = @CartId
          AND   c.TenantId = @TenantId
          AND   p.TenantId = @TenantId
        ORDER BY p.Name;";

    public async Task<IEnumerable<CartItemDto>> GetItemsAsync(Guid cartId)
    {
        using var conn = Factory.Create();
        return await conn.QueryAsync<CartItemDto>(GetItemsSql, new { CartId = cartId, TenantId });
    }

    public async Task<CartItem?> GetItemByProductAsync(Guid cartId, Guid productId)
    {
        using var conn = Factory.Create();
        return await conn.QuerySingleOrDefaultAsync<CartItem>(@"
            SELECT ci.Id, ci.CartId, ci.ProductId, ci.Quantity
            FROM   CartItems ci
            JOIN   Carts c ON c.Id = ci.CartId
            WHERE  ci.CartId = @CartId AND ci.ProductId = @ProductId
              AND  c.TenantId = @TenantId;",
            new { CartId = cartId, ProductId = productId, TenantId });
    }

    public async Task<CartItem?> GetItemByIdAsync(Guid cartId, Guid itemId)
    {
        using var conn = Factory.Create();
        return await conn.QuerySingleOrDefaultAsync<CartItem>(@"
            SELECT ci.Id, ci.CartId, ci.ProductId, ci.Quantity
            FROM   CartItems ci
            JOIN   Carts c ON c.Id = ci.CartId
            WHERE  ci.Id = @ItemId AND ci.CartId = @CartId
              AND  c.TenantId = @TenantId;",
            new { ItemId = itemId, CartId = cartId, TenantId });
    }

    public async Task AddItemAsync(Guid cartId, Guid productId, int quantity)
    {
        using var conn = Factory.Create();
        await conn.ExecuteAsync(@"
            INSERT INTO CartItems (CartId, ProductId, Quantity)
            VALUES (@CartId, @ProductId, @Quantity);

            UPDATE Carts SET UpdatedAt = SYSUTCDATETIME() WHERE Id = @CartId;",
            new { CartId = cartId, ProductId = productId, Quantity = quantity });
    }

    public async Task UpdateItemQuantityAsync(Guid itemId, int quantity)
    {
        using var conn = Factory.Create();
        await conn.ExecuteAsync(
            "UPDATE CartItems SET Quantity = @Quantity WHERE Id = @ItemId;",
            new { ItemId = itemId, Quantity = quantity });
    }

    public async Task RemoveItemAsync(Guid itemId)
    {
        using var conn = Factory.Create();
        await conn.ExecuteAsync(
            "DELETE FROM CartItems WHERE Id = @ItemId;", new { ItemId = itemId });
    }

    public async Task ClearAsync(Guid cartId)
    {
        using var conn = Factory.Create();
        await conn.ExecuteAsync(
            "DELETE FROM CartItems WHERE CartId = @CartId;", new { CartId = cartId });
    }
}
