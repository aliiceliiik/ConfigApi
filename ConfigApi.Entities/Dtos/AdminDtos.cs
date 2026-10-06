namespace ConfigApi.Entities.Dtos;

public class TenantListItemDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = "";
    public string Slug { get; set; } = "";
    public bool IsActive { get; set; }
    public int UserCount { get; set; }
    public int OrderCount { get; set; }
}

public class UserListItemDto
{
    public Guid Id { get; set; }
    public string Email { get; set; } = "";
    public string FullName { get; set; } = "";
    public string Role { get; set; } = "";
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public int OrderCount { get; set; }
}

public class UpdateOrderStatusRequest
{
    public byte Status { get; set; }
}
