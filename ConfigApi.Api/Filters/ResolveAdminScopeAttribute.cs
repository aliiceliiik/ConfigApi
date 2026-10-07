using ConfigApi.Business.Tenancy;
using ConfigApi.Context.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ConfigApi.Api.Filters;

public class ResolveAdminScopeAttribute : TypeFilterAttribute
{
    public const string TenantHeader = "X-Tenant-Id";

    public ResolveAdminScopeAttribute() : base(typeof(Filter)) { }

    private class Filter : IAsyncActionFilter
    {
        private readonly ITenantContext _tenant;
        private readonly ITenantRepository _tenants;

        public Filter(ITenantContext tenant, ITenantRepository tenants)
        {
            _tenant = tenant;
            _tenants = tenants;
        }

        public async Task OnActionExecutionAsync(
            ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if (!_tenant.IsSuperAdmin)
            {
                await next();
                return;
            }

            var header = context.HttpContext.Request.Headers[TenantHeader].FirstOrDefault();

            if (!Guid.TryParse(header, out var tenantId))
            {
                context.Result = new BadRequestObjectResult(new
                {
                    message = $"Süper admin için {TenantHeader} başlığı zorunlu."
                });
                return;
            }

            var tenant = await _tenants.GetByIdAsync(tenantId);
            if (tenant is null)
            {
                context.Result = new NotFoundObjectResult(new { message = "Tenant bulunamadı." });
                return;
            }

            _tenant.ImpersonateTenant(tenant.Id, tenant.Name);
            await next();
        }
    }
}
