namespace ConfigApi.Entities.Entities;

public class Company
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string AllowedDomains { get; set; } = "";
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}