using ConfigApi.Context.Factory;

namespace ConfigApi.Context.Repositories.Base;

public abstract class TenantScopedRepository
{
    protected readonly IDbConnectionFactory Factory;
    private readonly ITenantProvider _tenantProvider;

    protected TenantScopedRepository(IDbConnectionFactory factory, ITenantProvider tenantProvider)
    {
        Factory = factory;
        _tenantProvider = tenantProvider;
    }

    protected Guid TenantId => _tenantProvider.TenantId;
}
