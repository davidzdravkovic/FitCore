using FitCore.Api.Domain.Tenants;

namespace FitCore.Api.Domain.Staffs;

public class StaffInvite
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }
    public Tenant Tenant { get; set; } = null!;

    public Guid StaffId { get; set; }
    public Staff Staff { get; set; } = null!;

    public required string TokenHash { get; set; }
    public DateTime ExpiresAt { get; set; }
    public DateTime? UsedAt { get; set; }
    public DateTime CreatedAt { get; set; }
}
