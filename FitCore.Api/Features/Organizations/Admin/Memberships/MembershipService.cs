using FitCore.Api.Data.Stores.Locking;
using FitCore.Api.Data.Stores.OrganizationOwner.MembershipsStore;
using FitCore.Api.Domain.Members;
using FitCore.Api.Domain.Memberships;
using FitCore.Api.Domain.Plans;
using FitCore.Api.Domain.Visits;
using FitCore.Api.Errors.Business;
using FitCore.Api.Features.Organizations.Admin.Memberships.Assign;
using FitCore.Api.Features.Organizations.Admin.Memberships.Cancel;

namespace FitCore.Api.Features.Organizations.Admin.Memberships;

public class MembershipService(IMembershipStore membershipStore, IOrderedRowLocks rowLocks)
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
        await using var locks = await rowLocks.BeginAsync(cancellationToken);
        await locks.LockMemberAsync(tenantId, request.MemberId, cancellationToken);

        var member = await membershipStore.FindActiveMemberAsync(
            tenantId,
            request.MemberId,
            cancellationToken);

        if (member is null)
            return Result<MembershipResponse>.Fail(ErrorCodes.MemberNotFound);

        MemberTransitions.ActivateOnAssign(member);

        var plan = await membershipStore.FindActivePlanAsync(
            tenantId,
            request.PlanId,
            cancellationToken);

        if (plan is null)
            return Result<MembershipResponse>.Fail(ErrorCodes.PlanNotFound);

        if (plan.SessionCount is null or <= 0)
            return Result<MembershipResponse>.Fail(ErrorCodes.PlanNotFound);

        var startAt = request.StartAt?.ToUniversalTime() ?? DateTime.UtcNow;
        if (startAt.Kind == DateTimeKind.Unspecified)
            startAt = DateTime.SpecifyKind(startAt, DateTimeKind.Utc);

        var membership = new Membership
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            MemberId = member.Id,
            PlanId = plan.Id,
            Status = MembershipStatus.Active,
            StartAt = startAt,
            SessionTotal = plan.SessionCount.Value,
            SessionsReserved = 0,
            SessionsBurned = 0,
            CreatedAt = DateTime.UtcNow,
        };

        await membershipStore.AddAsync(membership);
        await membershipStore.SaveChangesAsync(cancellationToken);
        await locks.CommitAsync(cancellationToken);

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
        await using var locks = await rowLocks.BeginAsync(cancellationToken);

        await locks.LockMembershipAsync(tenantId, membershipId, cancellationToken);

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

        MembershipTransitions.Cancel(
            membership,
            reason,
            note,
            cancelledByStaffId,
            DateTime.UtcNow);

        var openVisits = await membershipStore.ListOpenVisitsForMembershipAsync(
            tenantId,
            membership.Id,
            cancellationToken);

        foreach (var visit in openVisits)
        {
            MembershipTransitions.ForfeitReservedOnVisitCancel(membership, visit);
            VisitTransitions.CancelOpen(visit);
        }

        await locks.LockMemberAsync(tenantId, membership.MemberId, cancellationToken);

        var activeLeft = await membershipStore.CountActiveMembershipsForMemberAsync(
            tenantId,
            membership.MemberId,
            membership.Id,
            cancellationToken);

        MemberTransitions.PauseIfNoOtherActive(membership.Member, activeLeft);

        await membershipStore.SaveChangesAsync(cancellationToken);
        await locks.CommitAsync(cancellationToken);

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
            membership.SessionTotal,
            membership.SessionsReserved,
            membership.SessionsBurned,
            membership.SessionsAvailable,
            membership.CreatedAt,
            membership.CancelReason?.ToString(),
            membership.CancelNote,
            membership.CancelledAt);
}
