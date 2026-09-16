namespace FitCore.Api.Infrastructure.Tenancy;

public sealed class TenantContext : ITenantContext
{
    private Guid? _tenantId;

    public Guid TenantId =>
        _tenantId
        ?? throw new InvalidOperationException(
            "Tenant context was not set. Apply [RequireActiveTenant] on the endpoint.");

    public bool IsSet => _tenantId.HasValue;

    public void Set(Guid tenantId)
    {
        if (_tenantId.HasValue)
            throw new InvalidOperationException("Tenant context was already set for this request.");

        _tenantId = tenantId;
    }
}
