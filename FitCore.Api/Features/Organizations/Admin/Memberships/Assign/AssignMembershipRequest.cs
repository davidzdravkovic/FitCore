using System.ComponentModel.DataAnnotations;

namespace FitCore.Api.Features.Organizations.Admin.Memberships.Assign;

public record AssignMembershipRequest(
    [Required] Guid MemberId,
    [Required] Guid PlanId,
    DateTime? StartAt = null);
