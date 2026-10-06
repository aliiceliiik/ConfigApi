using ConfigApi.Business.Tenancy;
using ConfigApi.Context.Repositories;
using ConfigApi.Entities.Dtos;
using ConfigApi.Entities.Enums;

namespace ConfigApi.Business.Services;

public interface IOrderService
{
    Task<(bool success, string? message, OrderDto? order)> CheckoutAsync();
    Task<IEnumerable<OrderListItemDto>> GetMyOrdersAsync();
    Task<OrderDto?> GetMyOrderDetailAsync(Guid orderId);
}

public class OrderService : IOrderService
{
    private readonly IOrderRepository _orders;
    private readonly ICartRepository _carts;
    private readonly ITenantContext _tenant;

    public OrderService(IOrderRepository orders, ICartRepository carts, ITenantContext tenant)
    {
        _orders = orders;
        _carts = carts;
        _tenant = tenant;
    }

    public async Task<(bool success, string? message, OrderDto? order)> CheckoutAsync()
    {
        var cart = await _carts.GetOrCreateForCurrentUserAsync(_tenant.UserId);

        try
        {
            var (orderId, _) = await _orders.CreateFromCartAsync(_tenant.UserId, cart.Id);
            var detail = await _orders.GetDetailAsync(orderId, _tenant.UserId);

            if (detail is not null)
                detail.Status = StatusText(detail.Status);

            return (true, null, detail);
        }
        catch (InvalidOperationException ex)
        {
            return (false, ex.Message, null);
        }
    }

    public async Task<IEnumerable<OrderListItemDto>> GetMyOrdersAsync()
    {
        var orders = (await _orders.GetForUserAsync(_tenant.UserId)).ToList();
        foreach (var order in orders)
            order.Status = StatusText(order.Status);

        return orders;
    }

    public async Task<OrderDto?> GetMyOrderDetailAsync(Guid orderId)
    {
        var order = await _orders.GetDetailAsync(orderId, _tenant.UserId);
        if (order is not null)
            order.Status = StatusText(order.Status);

        return order;
    }

    internal static string StatusText(string raw)
    {
        if (!byte.TryParse(raw, out var value) || !Enum.IsDefined(typeof(OrderStatus), value))
            return raw;

        return (OrderStatus)value switch
        {
            OrderStatus.Pending => "Beklemede",
            OrderStatus.Paid => "Ödendi",
            OrderStatus.Shipped => "Kargoda",
            OrderStatus.Completed => "Tamamlandı",
            OrderStatus.Cancelled => "İptal",
            _ => raw
        };
    }
}
