namespace ConfigApi.Entities.Entities;

public class Order
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid UserId { get; set; }
    public string OrderNumber { get; set; } = "";
    public decimal TotalAmount { get; set; }
    public byte Status { get; set; }
    public DateTime CreatedAt { get; set; }
}
