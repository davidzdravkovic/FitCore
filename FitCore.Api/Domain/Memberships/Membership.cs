using FitCore.Api.Domain.Members;
using FitCore.Api.Domain.Plans;
using FitCore.Api.Domain.Staffs;
using FitCore.Api.Domain.Tenants;

namespace FitCore.Api.Domain.Memberships;

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

    /// <summary>Frozen plan session grant at assign. Immutable.</summary>
    public int SessionTotal { get; set; }

    /// <summary>Credits held by open scheduled visits.</summary>
    public int SessionsReserved { get; set; }

    /// <summary>Credits resolved (check-in, forfeit cancel, etc.).</summary>
    public int SessionsBurned { get; set; }

    public int SessionsAvailable => SessionTotal - SessionsReserved - SessionsBurned;

    public DateTime CreatedAt { get; set; }

    public MembershipCancelReason? CancelReason { get; set; }
    public string? CancelNote { get; set; }
    public DateTime? CancelledAt { get; set; }
    public Guid? CancelledByStaffId { get; set; }
    public Staff? CancelledByStaff { get; set; }
}
