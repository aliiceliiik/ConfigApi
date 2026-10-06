namespace ConfigApi.Entities.Dtos;

public class ConfigResponse
{
    public Guid TenantId { get; set; }
    public string TenantName { get; set; } = "";
    public Dictionary<string, string> Settings { get; set; } = new();
}
