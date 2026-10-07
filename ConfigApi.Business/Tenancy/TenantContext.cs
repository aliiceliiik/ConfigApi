using System.Security.Claims;
using ConfigApi.Context.Repositories;
using ConfigApi.Entities.Enums;
using Microsoft.AspNetCore.Http;

namespace ConfigApi.Business.Tenancy;

public class TenantContext : ITenantContext, ITenantProvider
{
    private readonly IHttpContextAccessor _accessor;

    private Guid? _overrideTenantId;
    private string? _overrideTenantName;

    public TenantContext(IHttpContextAccessor accessor) => _accessor = accessor;

    private ClaimsPrincipal? User => _accessor.HttpContext?.User;

    public string Role => User?.FindFirst(ClaimTypes.Role)?.Value ?? "";

    public bool IsSuperAdmin => Role == UserRole.SuperAdmin;

    public Guid UserId =>
        Guid.TryParse(User?.FindFirst("userId")?.Value, out var id)
            ? id
            : throw new InvalidOperationException("Kimlik doğrulanmamış istek.");

    private Guid? TenantIdFromClaim =>
        Guid.TryParse(User?.FindFirst("tenantId")?.Value, out var id) ? id : null;

    public bool HasTenant => _overrideTenantId is not null || TenantIdFromClaim is not null;

    public Guid TenantId =>
        _overrideTenantId
        ?? TenantIdFromClaim
        ?? throw new InvalidOperationException(
            "Bu istek bir tenant kapsamında değil.");

    public string TenantName =>
        _overrideTenantName
        ?? User?.FindFirst("tenantName")?.Value
        ?? "";

    public void ImpersonateTenant(Guid tenantId, string? tenantName = null)
    {
        if (!IsSuperAdmin)
            throw new UnauthorizedAccessException("Tenant değiştirme yetkiniz yok.");

        _overrideTenantId = tenantId;
        _overrideTenantName = tenantName;
    }
}
