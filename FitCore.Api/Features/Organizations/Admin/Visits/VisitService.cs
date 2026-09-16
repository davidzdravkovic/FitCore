using FitCore.Api.Data.Stores.Locking;
using FitCore.Api.Data.Stores.OrganizationOwner.VisitsStore;
using FitCore.Api.Domain.Memberships;
using FitCore.Api.Domain.Plans;
using FitCore.Api.Domain.Visits;
using FitCore.Api.Errors.Business;
using FitCore.Api.Features.Organizations.Admin.Visits.Create;
using FitCore.Api.Features.Organizations.Admin.Visits.Void;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace FitCore.Api.Features.Organizations.Admin.Visits;

public class VisitService(IVisitStore visitStore, IOrderedRowLocks rowLocks)
{
    private const int DefaultWindowDays = 30;

    public async Task<IReadOnlyList<VisitResponse>> ListAsync(
        Guid tenantId,
        DateTime? from,
        DateTime? to,
        CancellationToken cancellationToken = default)
    {
        var windowStart = from is null ? DateTime.UtcNow.Date : ToUtc(from.Value);
        var windowEnd = to is null
            ? windowStart.AddDays(DefaultWindowDays)
            : ToUtc(to.Value);

        if (windowEnd <= windowStart)
            windowEnd = windowStart.AddDays(DefaultWindowDays);

        var visits = await visitStore.ListByTenantAsync(
            tenantId,
            windowStart,
            windowEnd,
            cancellationToken);

        return visits.Select(ToResponse).ToList();
    }

    public async Task<Result<VisitResponse>> CreateAsync(
        Guid tenantId,
        CreateVisitRequest request,
        CancellationToken cancellationToken = default)
    {
        var startAt = ToUtc(request.StartAt);
        var endAt = ToUtc(request.EndAt);

        if (endAt <= startAt)
            return Result<VisitResponse>.Fail(ErrorCodes.InvalidVisitInterval);

        await using var locks = await rowLocks.BeginAsync(cancellationToken);

        await locks.LockMembershipAsync(tenantId, request.MembershipId, cancellationToken);

        var membership = await visitStore.FindMembershipForScheduleAsync(
            tenantId,
            request.MembershipId,
            cancellationToken);

        if (membership is null)
            return Result<VisitResponse>.Fail(ErrorCodes.MembershipNotFound);

// Holding lock on membership protects other code to drift the state of the member, since the domain rule now is -> cancelling a member
// requires cancelling all active memberships, if this membership is not active the discard will happen anyway, reason none-schedulable

        if (!MembershipStatusRules.Schedulable.Contains(membership.Status))
            return Result<VisitResponse>.Fail(ErrorCodes.MembershipNotSchedulable);

        await locks.LockMemberAsync(tenantId, membership.MemberId, cancellationToken);
        await locks.LockCoachAsync(tenantId, request.CoachStaffId, cancellationToken);

         var coach = await visitStore.FindActiveCoachAsync(
            tenantId,
            request.CoachStaffId,
            cancellationToken);

        if (coach is null)
            return Result<VisitResponse>.Fail(ErrorCodes.StaffNotFound);

        if (await visitStore.HasOpenMemberOverlapAsync(
                tenantId,
                membership.MemberId,
                startAt,
                endAt,
                cancellationToken))
        {
            return Result<VisitResponse>.Fail(ErrorCodes.MemberVisitConflict);
        }

        if (await visitStore.HasOpenCoachOverlapAsync(
                tenantId,
                coach.Id,
                startAt,
                endAt,
                cancellationToken))
        {
            return Result<VisitResponse>.Fail(ErrorCodes.CoachUnavailable);
        }

        var consumedCredit = false;

        if (membership.Plan.EntitlementType == PlanEntitlementType.SessionPack)
        {
            if (membership.SessionsRemaining is null or <= 0)
                return Result<VisitResponse>.Fail(ErrorCodes.NoSessionCredit);

            membership.SessionsRemaining--;
            consumedCredit = true;
        }
        else
        {
            if (startAt < membership.StartAt
                || (membership.EndAt is not null && startAt > membership.EndAt))
            {
                return Result<VisitResponse>.Fail(ErrorCodes.MembershipOutsideWindow);
            }
        }

        var visit = new Visit
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            MembershipId = membership.Id,
            MemberId = membership.MemberId,
            CoachStaffId = coach.Id,
            ServiceId = membership.Plan.ServiceId,
            StartAt = startAt,
            EndAt = endAt,
            Status = VisitStatus.Scheduled,
            ConsumedSessionCredit = consumedCredit,
            CreatedAt = DateTime.UtcNow,
        };

        await visitStore.AddAsync(visit);

        try
        {
            await visitStore.SaveChangesAsync(cancellationToken);
            await locks.CommitAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (IsUniqueViolation(ex))
        {
            return Result<VisitResponse>.Fail(ErrorCodes.VisitAlreadyScheduled);
        }

        visit.Membership = membership;
        visit.Member = membership.Member;
        visit.CoachStaff = coach;
        visit.Service = membership.Plan.Service;

        return Result<VisitResponse>.Success(ToResponse(visit));
    }

    public async Task<Result<VisitResponse>> VoidAsync(
        Guid tenantId,
        Guid visitId,
        Guid voidedByStaffId,
        VoidVisitRequest request,
        CancellationToken cancellationToken = default)
    {
        var membershipId = await visitStore.FindMembershipIdForVisitAsync(
            tenantId,
            visitId,
            cancellationToken);

        if (membershipId is null)
            return Result<VisitResponse>.Fail(ErrorCodes.VisitNotFound);

        await using var locks = await rowLocks.BeginAsync(cancellationToken);

        await locks.LockMembershipAsync(tenantId, membershipId.Value, cancellationToken);
        await locks.LockVisitAsync(tenantId, visitId, cancellationToken);

        var visit = await visitStore.FindByIdForVoidAsync(
            tenantId,
            visitId,
            cancellationToken);

        if (visit is null)
            return Result<VisitResponse>.Fail(ErrorCodes.VisitNotFound);

        if (visit.Status == VisitStatus.Voided)
        {
            await locks.CommitAsync(cancellationToken);
            return Result<VisitResponse>.Success(ToResponse(visit));
        }

        if (!VisitStatusRules.Voidable.Contains(visit.Status))
            return Result<VisitResponse>.Fail(ErrorCodes.VisitNotVoidable);

        if (visit.ConsumedSessionCredit && visit.Membership.SessionsRemaining is not null)
        {
            visit.Membership.SessionsRemaining++;
            visit.ConsumedSessionCredit = false;
        }

        visit.Status = VisitStatus.Voided;
        visit.VoidNote = string.IsNullOrWhiteSpace(request.Note) ? null : request.Note.Trim();
        visit.VoidedAt = DateTime.UtcNow;
        visit.VoidedByStaffId = voidedByStaffId;

        await visitStore.SaveChangesAsync(cancellationToken);
        await locks.CommitAsync(cancellationToken);

        return Result<VisitResponse>.Success(ToResponse(visit));
    }

    private static DateTime ToUtc(DateTime value)
    {
        var utc = value.Kind switch
        {
            DateTimeKind.Utc => value,
            DateTimeKind.Local => value.ToUniversalTime(),
            _ => DateTime.SpecifyKind(value, DateTimeKind.Utc),
        };

        return utc;
    }

    private static bool IsUniqueViolation(DbUpdateException ex) =>
        ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation };

    private static VisitResponse ToResponse(Visit visit) =>
        new(
            visit.Id,
            visit.MembershipId,
            visit.MemberId,
            $"{visit.Member.FirstName} {visit.Member.LastName}".Trim(),
            visit.CoachStaffId,
            $"{visit.CoachStaff.FirstName} {visit.CoachStaff.LastName}".Trim(),
            visit.ServiceId,
            visit.Service.Name,
            visit.StartAt,
            visit.EndAt,
            visit.Status.ToString(),
            visit.ConsumedSessionCredit,
            visit.CreatedAt,
            visit.VoidNote,
            visit.VoidedAt);
}
