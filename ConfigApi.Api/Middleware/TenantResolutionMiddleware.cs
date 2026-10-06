using ConfigApi.Context.Repositories;

namespace ConfigApi.Api.Middleware;

public class TenantResolutionMiddleware
{
    public const string TenantItemKey = "ResolvedTenant";

    private readonly RequestDelegate _next;

    public TenantResolutionMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context, ITenantRepository tenantRepository)
    {
        var host = context.Request.Host.Host.ToLowerInvariant();

        if (!host.StartsWith("admin."))
        {
            var tenant = await tenantRepository.GetByDomainAsync(host);
            if (tenant is not null)
                context.Items[TenantItemKey] = tenant;
        }

        await _next(context);
    }
}
