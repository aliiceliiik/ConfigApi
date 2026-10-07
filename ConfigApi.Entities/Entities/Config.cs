namespace ConfigApi.Entities.Entities;

public class Config
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string ConfigKey { get; set; } = "";
    public string ConfigValue { get; set; } = "";
    public bool IsActive { get; set; }
}
