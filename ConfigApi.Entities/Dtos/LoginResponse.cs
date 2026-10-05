namespace ConfigApi.Entities.Dtos;

public class LoginResponse
{
    public string Token { get; set; } = "";
    public DateTime ExpiresAt { get; set; }
    public string CompanyName { get; set; } = "";
}