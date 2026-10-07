using System.Data;
using ConfigApi.Context.Factory;
using ConfigApi.Context.Repositories.Base;
using ConfigApi.Entities.Dtos;
using Dapper;

namespace ConfigApi.Context.Repositories;

public interface IOrderRepository
{
<<<<<<< HEAD
    Task<(Guid orderId, string orderNumber, IReadOnlyList<Guid> productIds)> CreateFromCartAsync(
        Guid userId, Guid cartId);
=======
    Task<(Guid orderId, string orderNumber)> CreateFromCartAsync(Guid userId, Guid cartId);
>>>>>>> bcdf37d9dda12d4a7aca61ef0d9fa06bf7e79f43
    Task<IEnumerable<OrderListItemDto>> GetForUserAsync(Guid userId);
    Task<IEnumerable<OrderListItemDto>> GetForTenantAsync();
    Task<OrderDto?> GetDetailAsync(Guid orderId, Guid? restrictToUserId);
    Task<bool> UpdateStatusAsync(Guid orderId, byte status);
}

public class OrderRepository : TenantScopedRepository, IOrderRepository
{
    public OrderRepository(IDbConnectionFactory f, ITenantProvider t) : base(f, t) { }

    private sealed class CheckoutLine
    {
        public Guid ProductId { get; set; }
        public string Name { get; set; } = "";
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public int Quantity { get; set; }
    }

    private const string ReadCartSql = @"
        SELECT  p.Id AS ProductId, p.Name, p.Price, p.Stock, ci.Quantity
        FROM    CartItems ci
        JOIN    Carts c    ON c.Id = ci.CartId
        JOIN    Products p WITH (UPDLOCK, ROWLOCK) ON p.Id = ci.ProductId
        WHERE   ci.CartId = @CartId
          AND   c.TenantId = @TenantId
          AND   c.UserId   = @UserId
          AND   p.TenantId = @TenantId
          AND   p.IsActive = 1;";

<<<<<<< HEAD
    public async Task<(Guid orderId, string orderNumber, IReadOnlyList<Guid> productIds)>
        CreateFromCartAsync(Guid userId, Guid cartId)
=======
    public async Task<(Guid orderId, string orderNumber)> CreateFromCartAsync(Guid userId, Guid cartId)
>>>>>>> bcdf37d9dda12d4a7aca61ef0d9fa06bf7e79f43
    {
        using var conn = Factory.Create();
        conn.Open();
        using var tx = conn.BeginTransaction(IsolationLevel.ReadCommitted);

        try
        {
            var lines = (await conn.QueryAsync<CheckoutLine>(
                ReadCartSql, new { CartId = cartId, TenantId, UserId = userId }, tx)).ToList();

            if (lines.Count == 0)
                throw new InvalidOperationException("Sepetiniz boş.");

            var yetersiz = lines.FirstOrDefault(l => l.Quantity > l.Stock);
            if (yetersiz is not null)
                throw new InvalidOperationException(
                    $"'{yetersiz.Name}' için yeterli stok yok. Mevcut: {yetersiz.Stock} adet.");

            var total = lines.Sum(l => l.Price * l.Quantity);
            var orderNumber = await GenerateOrderNumberAsync(conn, tx);

            var orderId = await conn.QuerySingleAsync<Guid>(@"
                INSERT INTO Orders (TenantId, UserId, OrderNumber, TotalAmount, Status)
                OUTPUT INSERTED.Id
                VALUES (@TenantId, @UserId, @OrderNumber, @Total, 0);",
                new { TenantId, UserId = userId, OrderNumber = orderNumber, Total = total }, tx);

            await conn.ExecuteAsync(@"
                INSERT INTO OrderItems (OrderId, ProductId, ProductName, UnitPrice, Quantity)
                VALUES (@OrderId, @ProductId, @ProductName, @UnitPrice, @Quantity);",
                lines.Select(l => new
                {
                    OrderId = orderId,
                    ProductId = l.ProductId,
                    ProductName = l.Name,
                    UnitPrice = l.Price,
                    Quantity = l.Quantity
                }), tx);

            foreach (var line in lines)
            {
                var affected = await conn.ExecuteAsync(@"
                    UPDATE Products
                    SET    Stock = Stock - @Quantity
                    WHERE  Id = @ProductId AND TenantId = @TenantId AND Stock >= @Quantity;",
                    new { line.Quantity, line.ProductId, TenantId }, tx);

                if (affected == 0)
                    throw new InvalidOperationException(
                        $"'{line.Name}' stoğu az önce tükendi, lütfen sepetinizi güncelleyin.");
            }

            await conn.ExecuteAsync(
                "DELETE FROM CartItems WHERE CartId = @CartId;", new { CartId = cartId }, tx);

            tx.Commit();
<<<<<<< HEAD
            return (orderId, orderNumber, lines.Select(l => l.ProductId).ToList());
=======
            return (orderId, orderNumber);
>>>>>>> bcdf37d9dda12d4a7aca61ef0d9fa06bf7e79f43
        }
        catch
        {
            tx.Rollback();
            throw;
        }
    }

    private async Task<string> GenerateOrderNumberAsync(IDbConnection conn, IDbTransaction tx)
    {
        var prefix = DateTime.UtcNow.ToString("yyyyMMdd");
        var seq = await conn.ExecuteScalarAsync<int>(@"
            SELECT COUNT(*) + 1 FROM Orders
            WHERE TenantId = @TenantId AND OrderNumber LIKE @Prefix;",
            new { TenantId, Prefix = $"{prefix}%" }, tx);

        return $"{prefix}-{seq:D4}";
    }

    public async Task<IEnumerable<OrderListItemDto>> GetForUserAsync(Guid userId)
    {
        using var conn = Factory.Create();
        return await conn.QueryAsync<OrderListItemDto>(@"
            SELECT  o.Id, o.OrderNumber, o.TotalAmount, o.CreatedAt,
                    CAST(o.Status AS NVARCHAR(10)) AS Status,
                    (SELECT COUNT(*) FROM OrderItems oi WHERE oi.OrderId = o.Id) AS ItemCount
            FROM    Orders o
            WHERE   o.TenantId = @TenantId AND o.UserId = @UserId
            ORDER BY o.CreatedAt DESC;",
            new { TenantId, UserId = userId });
    }

    public async Task<IEnumerable<OrderListItemDto>> GetForTenantAsync()
    {
        using var conn = Factory.Create();
        return await conn.QueryAsync<OrderListItemDto>(@"
            SELECT  o.Id, o.OrderNumber, o.TotalAmount, o.CreatedAt,
                    CAST(o.Status AS NVARCHAR(10)) AS Status,
                    u.FullName AS CustomerName,
                    (SELECT COUNT(*) FROM OrderItems oi WHERE oi.OrderId = o.Id) AS ItemCount
            FROM    Orders o
            JOIN    Users u ON u.Id = o.UserId
            WHERE   o.TenantId = @TenantId
            ORDER BY o.CreatedAt DESC;",
            new { TenantId });
    }

    public async Task<OrderDto?> GetDetailAsync(Guid orderId, Guid? restrictToUserId)
    {
        using var conn = Factory.Create();

        using var multi = await conn.QueryMultipleAsync(@"
            SELECT  o.Id, o.OrderNumber, o.TotalAmount, o.CreatedAt,
                    CAST(o.Status AS NVARCHAR(10)) AS Status
            FROM    Orders o
            WHERE   o.Id = @OrderId AND o.TenantId = @TenantId
              AND   (@UserId IS NULL OR o.UserId = @UserId);

            SELECT  oi.ProductId, oi.ProductName, oi.UnitPrice, oi.Quantity
            FROM    OrderItems oi
            JOIN    Orders o ON o.Id = oi.OrderId
            WHERE   oi.OrderId = @OrderId AND o.TenantId = @TenantId
              AND   (@UserId IS NULL OR o.UserId = @UserId);",
            new { OrderId = orderId, TenantId, UserId = restrictToUserId });

        var order = await multi.ReadSingleOrDefaultAsync<OrderDto>();
        if (order is null) return null;

        order.Items = (await multi.ReadAsync<OrderItemDto>()).ToList();
        return order;
    }

    public async Task<bool> UpdateStatusAsync(Guid orderId, byte status)
    {
        using var conn = Factory.Create();
        var affected = await conn.ExecuteAsync(@"
            UPDATE Orders SET Status = @Status
            WHERE  Id = @OrderId AND TenantId = @TenantId;",
            new { OrderId = orderId, TenantId, Status = status });

        return affected > 0;
    }
}
