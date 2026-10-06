using System.ComponentModel.DataAnnotations;

namespace ConfigApi.Entities.Dtos;

public class AddToCartRequest
{
    [Required]
    public Guid ProductId { get; set; }

    [Range(1, 100, ErrorMessage = "Miktar 1 ile 100 arasında olmalı.")]
    public int Quantity { get; set; } = 1;
}

public class UpdateCartItemRequest
{
    [Range(1, 100)]
    public int Quantity { get; set; }
}

public class CartItemDto
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = "";
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public int AvailableStock { get; set; }
    public decimal LineTotal => UnitPrice * Quantity;
}

public class CartDto
{
    public Guid Id { get; set; }
    public List<CartItemDto> Items { get; set; } = [];
    public decimal Total => Items.Sum(i => i.LineTotal);
    public int ItemCount => Items.Sum(i => i.Quantity);
}
