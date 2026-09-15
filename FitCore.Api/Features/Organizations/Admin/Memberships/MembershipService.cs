using FitCore.Api.Data.Stores.OrganizationOwner.MembershipsStore;
using FitCore.Api.Domain.Members;
using FitCore.Api.Domain.Memberships;
using FitCore.Api.Domain.Plans;
using FitCore.Api.Domain.Tenants;
using FitCore.Api.Errors.Business;
using FitCore.Api.Features.Organizations.Admin.Memberships.Assign;
using FitCore.Api.Features.Organizations.Admin.Memberships.Cancel;

namespace FitCore.Api.Features.Organizations.Admin.Memberships;

public class MembershipService(IMembershipStore membershipStore)
{
    public async Task<IReadOnlyList<MembershipResponse>> ListAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        var memberships = await membershipStore.ListByTenantAsync(tenantId, cancellationToken);
        return memberships.Select(ToResponse).ToList();
    }

    public async Task<Result<MembershipResponse>> AssignAsync(
        Guid tenantId,
        AssignMembershipRequest request,
        CancellationToken cancellationToken = default)
    {
        var tenant = await membershipStore.FindTenantByIdAsync(tenantId, cancellationToken);

        if (tenant is null)
            return Result<MembershipResponse>.Fail(ErrorCodes.OrganizationNotFound);

        if (tenant.Status != TenantStatus.Active)
            return Result<MembershipResponse>.Fail(ErrorCodes.OrganizationNotActive);

        var member = await membershipStore.FindActiveMemberAsync(
            tenantId,
            request.MemberId,
            cancellationToken);

        if (member is null)
            return Result<MembershipResponse>.Fail(ErrorCodes.MemberNotFound);

        if (MemberStatusRules.PromoteToActiveOnAssign.Contains(member.Status))
            member.Status = MemberStatus.Active;

        var plan = await membershipStore.FindActivePlanAsync(
            tenantId,
            request.PlanId,
            cancellationToken);

        if (plan is null)
            return Result<MembershipResponse>.Fail(ErrorCodes.PlanNotFound);

        var startAt = request.StartAt?.ToUniversalTime() ?? DateTime.UtcNow;
        if (startAt.Kind == DateTimeKind.Unspecified)
            startAt = DateTime.SpecifyKind(startAt, DateTimeKind.Utc);

        int? sessionsRemaining = null;
        DateTime? endAt = null;

        if (plan.EntitlementType == PlanEntitlementType.SessionPack)
        {
            sessionsRemaining = plan.SessionCount;
        }
        else
        {
            endAt = startAt.AddDays(plan.DurationDays!.Value);
        }

        var membership = new Membership
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            MemberId = member.Id,
            PlanId = plan.Id,
            Status = MembershipStatus.Active,
            StartAt = startAt,
            EndAt = endAt,
            SessionsRemaining = sessionsRemaining,
            CreatedAt = DateTime.UtcNow,
        };

        await membershipStore.AddAsync(membership);
        await membershipStore.SaveChangesAsync(cancellationToken);

        membership.Member = member;
        membership.Plan = plan;

        return Result<MembershipResponse>.Success(ToResponse(membership));
    }

    public async Task<Result<MembershipResponse>> CancelAsync(
        Guid tenantId,
        Guid membershipId,
        Guid cancelledByStaffId,
        CancelMembershipRequest request,
        CancellationToken cancellationToken = default)
    {
        var tenant = await membershipStore.FindTenantByIdAsync(tenantId, cancellationToken);

        if (tenant is null)
            return Result<MembershipResponse>.Fail(ErrorCodes.OrganizationNotFound);

        if (tenant.Status != TenantStatus.Active)
            return Result<MembershipResponse>.Fail(ErrorCodes.OrganizationNotActive);

        var membership = await membershipStore.FindByIdForCancelAsync(
            tenantId,
            membershipId,
            cancellationToken);

        if (membership is null)
            return Result<MembershipResponse>.Fail(ErrorCodes.MembershipNotFound);

        if (!MembershipStatusRules.Cancellable.Contains(membership.Status))
            return Result<MembershipResponse>.Fail(ErrorCodes.MembershipNotCancellable);

        var reason = Enum.Parse<MembershipCancelReason>(request.Reason.Trim(), ignoreCase: true);
        var note = string.IsNullOrWhiteSpace(request.Note) ? null : request.Note.Trim();

        membership.Status = MembershipStatus.Cancelled;
        membership.CancelReason = reason;
        membership.CancelNote = note;
        membership.CancelledAt = DateTime.UtcNow;
        membership.CancelledByStaffId = cancelledByStaffId;

        var activeLeft = await membershipStore.CountActiveMembershipsForMemberAsync(
            tenantId,
            membership.MemberId,
            membership.Id,
            cancellationToken);

        if (activeLeft == 0
            && membership.Member.Status == MemberStatus.Active)
        {
            membership.Member.Status = MemberStatus.Paused;
        }

        await membershipStore.SaveChangesAsync(cancellationToken);

        return Result<MembershipResponse>.Success(ToResponse(membership));
    }

    private static MembershipResponse ToResponse(Membership membership) =>
        new(
            membership.Id,
            membership.MemberId,
            $"{membership.Member.FirstName} {membership.Member.LastName}".Trim(),
            membership.PlanId,
            membership.Plan.Name,
            membership.Status.ToString(),
            membership.StartAt,
            membership.EndAt,
            membership.SessionsRemaining,
            membership.CreatedAt,
            membership.CancelReason?.ToString(),
            membership.CancelNote,
            membership.CancelledAt);
}
