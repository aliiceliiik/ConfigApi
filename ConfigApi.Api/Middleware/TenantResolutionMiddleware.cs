using ConfigApi.Context.Caching;
using ConfigApi.Context.Repositories;

namespace ConfigApi.Api.Middleware;

public class TenantResolutionMiddleware
{
    public const string TenantItemKey = "ResolvedTenant";
    public const string TenantHostItemKey = "ResolvedTenantHost";
    public const string TenantHostHeader = "X-Tenant-Host";

    private static readonly TimeSpan CacheTtl = TimeSpan.FromMinutes(60);

    private readonly RequestDelegate _next;

    public TenantResolutionMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context,
                                  ITenantRepository tenantRepository,
                                  ICacheService cache)
    {
        var header = context.Request.Headers[TenantHostHeader].FirstOrDefault();

        var host = (string.IsNullOrWhiteSpace(header)
            ? context.Request.Host.Host
            : header).ToLowerInvariant();

        context.Items[TenantHostItemKey] = host;

        if (!host.StartsWith("admin."))
        {
            var tenant = await cache.GetOrSetAsync(
                CacheKeys.TenantByDomain(host),
                CacheTtl,
                () => tenantRepository.GetByDomainAsync(host));

            if (tenant is not null)
                context.Items[TenantItemKey] = tenant;
        }

        await _next(context);
    }
}
