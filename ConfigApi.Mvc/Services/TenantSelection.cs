namespace ConfigApi.Mvc.Services;

public interface ITenantSelection
{
    Guid? SelectedTenantId { get; }
    string? SelectedTenantName { get; }
    void Select(Guid tenantId, string tenantName);
    void Clear();
}

public class TenantSelection : ITenantSelection
{
    private const string IdKey = "selected_tenant_id";
    private const string NameKey = "selected_tenant_name";

    private readonly IHttpContextAccessor _accessor;

    public TenantSelection(IHttpContextAccessor accessor) => _accessor = accessor;

    private ISession? Session => _accessor.HttpContext?.Session;

    public Guid? SelectedTenantId =>
        Guid.TryParse(Session?.GetString(IdKey), out var id) ? id : null;

    public string? SelectedTenantName => Session?.GetString(NameKey);

    public void Select(Guid tenantId, string tenantName)
    {
        Session?.SetString(IdKey, tenantId.ToString());
        Session?.SetString(NameKey, tenantName);
    }

    public void Clear()
    {
        Session?.Remove(IdKey);
        Session?.Remove(NameKey);
    }
}
