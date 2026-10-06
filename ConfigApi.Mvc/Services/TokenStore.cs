namespace ConfigApi.Mvc.Services;

public interface ITokenStore
{
    string? GetAccessToken();
    string? GetRefreshToken();
    void Save(string accessToken, string refreshToken, DateTime expiresAt);
    void Clear();
}

public class TokenStore : ITokenStore
{
    private const string AccessKey = "access_token";
    private const string RefreshCookie = "rt";

    private readonly IHttpContextAccessor _accessor;

    public TokenStore(IHttpContextAccessor accessor) => _accessor = accessor;

    private HttpContext Ctx => _accessor.HttpContext
        ?? throw new InvalidOperationException("HttpContext bulunamadı.");

    public string? GetAccessToken() => Ctx.Session.GetString(AccessKey);

    public string? GetRefreshToken() => Ctx.Request.Cookies[RefreshCookie];

    public void Save(string accessToken, string refreshToken, DateTime expiresAt)
    {
        Ctx.Session.SetString(AccessKey, accessToken);

        Ctx.Response.Cookies.Append(RefreshCookie, refreshToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = Ctx.Request.IsHttps,
            SameSite = SameSiteMode.Lax,
            IsEssential = true,
            Expires = DateTimeOffset.UtcNow.AddDays(7)
        });
    }

    public void Clear()
    {
        Ctx.Session.Remove(AccessKey);
        Ctx.Response.Cookies.Delete(RefreshCookie);
    }
}
