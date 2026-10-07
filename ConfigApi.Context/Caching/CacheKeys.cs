using System.Security.Cryptography;
using System.Text;

namespace ConfigApi.Context.Caching;

public static class CacheKeys
{
    public static string TenantPrefix(Guid tenantId) => $"t:{tenantId}:";

    public static string ProductSearch(Guid tenantId, string signature)
        => $"{TenantPrefix(tenantId)}products:search:{Hash(signature)}";

    public static string ProductSearchPrefix(Guid tenantId)
        => $"{TenantPrefix(tenantId)}products:";

    public static string Config(Guid tenantId)
        => $"{TenantPrefix(tenantId)}config";

    public static string Cart(Guid tenantId, Guid userId)
        => $"{TenantPrefix(tenantId)}cart:{userId}";

    public static string TenantByDomain(string domain)
        => $"tenant:domain:{domain.ToLowerInvariant()}";

    private static string Hash(string input)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(bytes)[..16].ToLowerInvariant();
    }
}
