<<<<<<< HEAD
﻿using ConfigApi.Context.Caching;
using ConfigApi.Context.Repositories;
=======
﻿using ConfigApi.Context.Repositories;
>>>>>>> bcdf37d9dda12d4a7aca61ef0d9fa06bf7e79f43

namespace ConfigApi.Api.Middleware;

public class TenantResolutionMiddleware
{
    public const string TenantItemKey = "ResolvedTenant";
<<<<<<< HEAD
    public const string TenantHostItemKey = "ResolvedTenantHost";
    public const string TenantHostHeader = "X-Tenant-Host";

    private static readonly TimeSpan CacheTtl = TimeSpan.FromMinutes(60);
=======
>>>>>>> bcdf37d9dda12d4a7aca61ef0d9fa06bf7e79f43

    private readonly RequestDelegate _next;

    public TenantResolutionMiddleware(RequestDelegate next) => _next = next;

<<<<<<< HEAD
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

=======
    public async Task InvokeAsync(HttpContext context, ITenantRepository tenantRepository)
    {
        var host = context.Request.Host.Host.ToLowerInvariant();

        if (!host.StartsWith("admin."))
        {
            var tenant = await tenantRepository.GetByDomainAsync(host);
>>>>>>> bcdf37d9dda12d4a7aca61ef0d9fa06bf7e79f43
            if (tenant is not null)
                context.Items[TenantItemKey] = tenant;
        }

        await _next(context);
    }
}
