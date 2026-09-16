namespace FitCore.Api.Infrastructure.Tenancy;

/// <summary>
/// Request-scoped tenant after <see cref="RequireActiveTenantAttribute"/> has run.
/// </summary>
public interface ITenantContext
{
    Guid TenantId { get; }

    bool IsSet { get; }

    void Set(Guid tenantId);
}
