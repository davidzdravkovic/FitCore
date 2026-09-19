using FitCore.Api.Domain.Memberships;
using FitCore.Api.Domain.Services;
using FitCore.Api.Domain.Tenants;

namespace FitCore.Api.Domain.Plans;

public class MembershipPlan
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }
    public Tenant Tenant { get; set; } = null!;

    public Guid ServiceId { get; set; }
    public Service Service { get; set; } = null!;

    public string Name { get; set; } = null!;
    public decimal Price { get; set; }

    public PlanEntitlementType EntitlementType { get; set; }

    public int? SessionCount { get; set; }

    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }

    public ICollection<Membership> Memberships { get; set; } = [];
}
