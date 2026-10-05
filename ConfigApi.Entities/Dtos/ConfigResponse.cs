namespace ConfigApi.Entities.Dtos;

public class ConfigResponse
{
    public int CompanyId { get; set; }
    public string CompanyName { get; set; } = "";
    public Dictionary<string, string> Settings { get; set; } = new();
}