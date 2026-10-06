namespace ConfigApi.Context.Repositories;

public interface ITenantProvider
{
    Guid TenantId { get; }
}
