namespace ConfigApi.Entities.Entities;

public class Config
{
    public int Id { get; set; }
    public int CompanyId { get; set; }
    public string ConfigKey { get; set; } = "";
    public string ConfigValue { get; set; } = "";
    public bool IsActive { get; set; }
}