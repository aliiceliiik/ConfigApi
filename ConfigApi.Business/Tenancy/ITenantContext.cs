namespace ConfigApi.Business.Tenancy;

public interface ITenantContext
{
    Guid TenantId { get; }
    string TenantName { get; }
    Guid UserId { get; }
    string Role { get; }
    bool IsSuperAdmin { get; }
    bool HasTenant { get; }

    void ImpersonateTenant(Guid tenantId, string? tenantName = null);
}
