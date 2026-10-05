namespace ConfigApi.Business.Security;

public static class DomainHelper
{
    /// Origin/Referer header'ından host kısmını çıkarır.
    /// "https://www.x.com/login" → "www.x.com"
    public static string? Extract(string? originOrReferer)
    {
        if (string.IsNullOrWhiteSpace(originOrReferer))
            return null;

        return Uri.TryCreate(originOrReferer, UriKind.Absolute, out var uri)
            ? uri.Host.ToLowerInvariant()
            : null;
    }

    /// Domain, firmanın izinli listesinde mi?
    public static bool IsAllowed(string? domain, string allowedDomains)
    {
        if (string.IsNullOrWhiteSpace(domain))
            return false;

        return allowedDomains
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Any(d => string.Equals(d, domain, StringComparison.OrdinalIgnoreCase));
    }
}