using FitCore.Api.Domain.Members;
using FitCore.Api.Domain.Memberships;
using FitCore.Api.Domain.Services;
using FitCore.Api.Domain.Staffs;
using FitCore.Api.Domain.Tenants;

namespace FitCore.Api.Domain.Visits;

public class Visit
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }
    public Tenant Tenant { get; set; } = null!;

    public Guid MembershipId { get; set; }
    public Membership Membership { get; set; } = null!;

    public Guid MemberId { get; set; }
    public Member Member { get; set; } = null!;

    public Guid CoachStaffId { get; set; }
    public Staff CoachStaff { get; set; } = null!;

    public Guid ServiceId { get; set; }
    public Service Service { get; set; } = null!;

    public DateTime StartAt { get; set; }
    public DateTime EndAt { get; set; }

    public VisitStatus Status { get; set; }

    public bool ConsumedSessionCredit { get; set; }

    public DateTime CreatedAt { get; set; }

    public string? VoidNote { get; set; }
    public DateTime? VoidedAt { get; set; }
    public Guid? VoidedByStaffId { get; set; }
    public Staff? VoidedByStaff { get; set; }
}
