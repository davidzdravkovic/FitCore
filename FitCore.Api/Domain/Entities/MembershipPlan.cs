using FitCore.Api.Domain.Enums;

namespace FitCore.Api.Domain.Entities;

public class MembershipPlan
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }
    public Tenant Tenant { get; set; } = null!;

    public Guid ServiceId { get; set; }
    public Service Service { get; set; } = null!;

    public string Name { get; set; } = null!;
    public decimal Price { get; set; }
    public string Currency { get; set; } = "USD";

    public PlanEntitlementType EntitlementType { get; set; }

    /// <summary>For session packs: how many sessions the plan includes.</summary>
    public int? SessionCount { get; set; }

    /// <summary>For time-period plans: length of access in days (e.g. 30).</summary>
    public int? DurationDays { get; set; }

    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }

    public ICollection<Membership> Memberships { get; set; } = [];
}
