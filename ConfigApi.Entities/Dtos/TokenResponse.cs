namespace ConfigApi.Entities.Dtos;

public class TokenResponse
{
    public string AccessToken { get; set; } = "";
    public string RefreshToken { get; set; } = "";
    public DateTime ExpiresAt { get; set; }
    public string FullName { get; set; } = "";
    public string Role { get; set; } = "";
}
