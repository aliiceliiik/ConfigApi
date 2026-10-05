namespace ConfigApi.Entities.Dtos;

public class UserDto
{
    public int Id { get; set; }
    public string Email { get; set; } = "";
    public string PasswordHash { get; set; } = "";
    public bool IsActive { get; set; }
    public int CompanyId { get; set; }
    public string CompanyName { get; set; } = "";
    public string AllowedDomains { get; set; } = "";
}