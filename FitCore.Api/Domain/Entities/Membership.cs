using FitCore.Api.Domain.Enums;

namespace FitCore.Api.Domain.Entities;

public class Membership
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }
    public Tenant Tenant { get; set; } = null!;

    public Guid MemberId { get; set; }
    public Member Member { get; set; } = null!;

    public Guid PlanId { get; set; }
    public MembershipPlan Plan { get; set; } = null!;

    public MembershipStatus Status { get; set; }

    public DateTime StartAt { get; set; }
    public DateTime? EndAt { get; set; }

    /// <summary>Remaining sessions for pack plans; null for time-period plans.</summary>
    public int? SessionsRemaining { get; set; }

    public DateTime CreatedAt { get; set; }
}
