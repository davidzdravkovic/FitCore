using FitCore.Api.Data.Stores.Locking;
using FitCore.Api.Data.Stores.OrganizationOwner.VisitsStore;
using FitCore.Api.Domain.Members;
using FitCore.Api.Domain.Memberships;
using FitCore.Api.Domain.Visits;
using FitCore.Api.Errors.Business;
using FitCore.Api.Features.Organizations.Admin.Visits.Create;
using FitCore.Api.Features.Organizations.Admin.Visits.Record;
using FitCore.Api.Features.Organizations.Admin.Visits.Reschedule;
using FitCore.Api.Features.Organizations.Admin.Visits.Resolve;
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

        if (startAt < DateTime.UtcNow)
            return Result<VisitResponse>.Fail(ErrorCodes.VisitStartInPast);

        await using var locks = await rowLocks.BeginAsync(cancellationToken);

        await locks.LockMembershipAsync(tenantId, request.MembershipId, cancellationToken);

        var membership = await visitStore.FindMembershipForScheduleAsync(
            tenantId,
            request.MembershipId,
            cancellationToken);

        if (membership is null)
            return Result<VisitResponse>.Fail(ErrorCodes.MembershipNotFound);

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

        var reserved = MembershipTransitions.TryReserveEntitlementForSchedule(membership);

        if (!reserved.Succeeded)
            return Result<VisitResponse>.Fail(reserved.Error!);

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
            ConsumedSessionCredit = true,
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

    public async Task<Result<VisitResponse>> RecordAsync(
        Guid tenantId,
        RecordVisitRequest request,
        CancellationToken cancellationToken = default)
    {
        var outcome = request.Outcome switch
        {
            VisitResolveOutcome.Completed => VisitStatus.Completed,
            VisitResolveOutcome.NoShow => VisitStatus.NoShow,
            _ => (VisitStatus?)null,
        };

        if (outcome is null)
            return Result<VisitResponse>.Fail(ErrorCodes.VisitNotResolvable);

        var startAt = ToUtc(request.StartAt);
        var endAt = ToUtc(request.EndAt);

        if (endAt <= startAt)
            return Result<VisitResponse>.Fail(ErrorCodes.InvalidVisitInterval);

        if (startAt >= DateTime.UtcNow)
            return Result<VisitResponse>.Fail(ErrorCodes.VisitRecordRequiresPastStart);

        await using var locks = await rowLocks.BeginAsync(cancellationToken);

        await locks.LockMembershipAsync(tenantId, request.MembershipId, cancellationToken);

        var membership = await visitStore.FindMembershipForScheduleAsync(
            tenantId,
            request.MembershipId,
            cancellationToken);

        if (membership is null)
            return Result<VisitResponse>.Fail(ErrorCodes.MembershipNotFound);

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

        if (await visitStore.HasOccupyingMemberOverlapAsync(
                tenantId,
                membership.MemberId,
                startAt,
                endAt,
                cancellationToken))
        {
            return Result<VisitResponse>.Fail(ErrorCodes.MemberVisitConflict);
        }

        if (await visitStore.HasOccupyingCoachOverlapAsync(
                tenantId,
                coach.Id,
                startAt,
                endAt,
                cancellationToken))
        {
            return Result<VisitResponse>.Fail(ErrorCodes.CoachUnavailable);
        }

        var burned = MembershipTransitions.TryBurnAvailableOnRecord(membership);
        if (!burned.Succeeded)
            return Result<VisitResponse>.Fail(burned.Error!);

        if (membership.Status == MembershipStatus.Expired)
        {
            var otherActive = await visitStore.CountActiveMembershipsForMemberAsync(
                tenantId,
                membership.MemberId,
                membership.Id,
                cancellationToken);
            MemberTransitions.PauseIfNoOtherActive(membership.Member, otherActive);
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
            Status = outcome.Value,
            ConsumedSessionCredit = true,
            CreatedAt = DateTime.UtcNow,
        };

        await visitStore.AddAsync(visit);
        await visitStore.SaveChangesAsync(cancellationToken);
        await locks.CommitAsync(cancellationToken);

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
        var lockKeys = await visitStore.FindLockKeysForVisitAsync(
            tenantId,
            visitId,
            cancellationToken);

        if (lockKeys is null)
            return Result<VisitResponse>.Fail(ErrorCodes.VisitNotFound);

        await using var locks = await rowLocks.BeginAsync(cancellationToken);

        await locks.LockMembershipAsync(tenantId, lockKeys.MembershipId, cancellationToken);
        await locks.LockMemberAsync(tenantId, lockKeys.MemberId, cancellationToken);
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

        if (MembershipTransitions.TryRestorePackCreditOnVoid(visit))
            MemberTransitions.ActivateIfPaused(visit.Member);

        var note = string.IsNullOrWhiteSpace(request.Note) ? null : request.Note.Trim();
        VisitTransitions.Void(visit, note, voidedByStaffId, DateTime.UtcNow);

        await visitStore.SaveChangesAsync(cancellationToken);
        await locks.CommitAsync(cancellationToken);

        return Result<VisitResponse>.Success(ToResponse(visit));
    }

    public async Task<Result<VisitResponse>> ResolveScheduledAsync(
        Guid tenantId,
        Guid visitId,
        ResolveVisitRequest request,
        CancellationToken cancellationToken = default)
    {
        var outcome = request.Outcome switch
        {
            VisitResolveOutcome.Completed => VisitStatus.Completed,
            VisitResolveOutcome.NoShow => VisitStatus.NoShow,
            _ => (VisitStatus?)null,
        };

        if (outcome is null)
            return Result<VisitResponse>.Fail(ErrorCodes.VisitNotResolvable);

        var occurredAt = ToUtc(request.OccurredAt);

        var lockKeys = await visitStore.FindLockKeysForVisitAsync(
            tenantId,
            visitId,
            cancellationToken);

        if (lockKeys is null)
            return Result<VisitResponse>.Fail(ErrorCodes.VisitNotFound);

        await using var locks = await rowLocks.BeginAsync(cancellationToken);

        await locks.LockMembershipAsync(tenantId, lockKeys.MembershipId, cancellationToken);
        await locks.LockMemberAsync(tenantId, lockKeys.MemberId, cancellationToken);
        await locks.LockVisitAsync(tenantId, visitId, cancellationToken);

        var visit = await visitStore.FindByIdForVoidAsync(
            tenantId,
            visitId,
            cancellationToken);

        if (visit is null)
            return Result<VisitResponse>.Fail(ErrorCodes.VisitNotFound);

        if (visit.Status == outcome)
        {
            await locks.CommitAsync(cancellationToken);
            return Result<VisitResponse>.Success(ToResponse(visit));
        }

        if (!VisitStatusRules.Resolvable.Contains(visit.Status))
            return Result<VisitResponse>.Fail(ErrorCodes.VisitNotResolvable);

        if (occurredAt < visit.StartAt)
            return Result<VisitResponse>.Fail(ErrorCodes.VisitResolveBeforeStart);

        MembershipTransitions.BurnReservedOnComplete(visit.Membership, visit);

        if (visit.Membership.Status == MembershipStatus.Expired)
        {
            var otherActive = await visitStore.CountActiveMembershipsForMemberAsync(
                tenantId,
                visit.MemberId,
                visit.MembershipId,
                cancellationToken);
            MemberTransitions.PauseIfNoOtherActive(visit.Member, otherActive);
        }

        VisitTransitions.Resolve(visit, outcome.Value);

        await visitStore.SaveChangesAsync(cancellationToken);
        await locks.CommitAsync(cancellationToken);

        return Result<VisitResponse>.Success(ToResponse(visit));
    }

    public async Task<Result<VisitResponse>> RescheduleAsync(
        Guid tenantId,
        Guid visitId,
        RescheduleVisitRequest request,
        CancellationToken cancellationToken = default)
    {
        var startAt = ToUtc(request.StartAt);
        var endAt = ToUtc(request.EndAt);

        if (endAt <= startAt)
            return Result<VisitResponse>.Fail(ErrorCodes.InvalidVisitInterval);

        if (startAt < DateTime.UtcNow)
            return Result<VisitResponse>.Fail(ErrorCodes.VisitStartInPast);

        var lockKeys = await visitStore.FindLockKeysForVisitAsync(
            tenantId,
            visitId,
            cancellationToken);

        if (lockKeys is null)
            return Result<VisitResponse>.Fail(ErrorCodes.VisitNotFound);

        await using var locks = await rowLocks.BeginAsync(cancellationToken);

        await locks.LockMembershipAsync(tenantId, lockKeys.MembershipId, cancellationToken);
        await locks.LockMemberAsync(tenantId, lockKeys.MemberId, cancellationToken);

        var visit = await visitStore.FindByIdForVoidAsync(
            tenantId,
            visitId,
            cancellationToken);

        if (visit is null)
            return Result<VisitResponse>.Fail(ErrorCodes.VisitNotFound);

        if (!VisitStatusRules.Reschedulable.Contains(visit.Status))
            return Result<VisitResponse>.Fail(ErrorCodes.VisitNotReschedulable);

        await locks.LockCoachAsync(tenantId, request.CoachStaffId, cancellationToken);
        await locks.LockVisitAsync(tenantId, visitId, cancellationToken);

        var coach = await visitStore.FindActiveCoachAsync(
            tenantId,
            request.CoachStaffId,
            cancellationToken);

        if (coach is null)
            return Result<VisitResponse>.Fail(ErrorCodes.StaffNotFound);

        if (await visitStore.HasOpenMemberOverlapAsync(
                tenantId,
                visit.MemberId,
                startAt,
                endAt,
                cancellationToken,
                excludeVisitId: visit.Id))
        {
            return Result<VisitResponse>.Fail(ErrorCodes.MemberVisitConflict);
        }

        if (await visitStore.HasOpenCoachOverlapAsync(
                tenantId,
                coach.Id,
                startAt,
                endAt,
                cancellationToken,
                excludeVisitId: visit.Id))
        {
            return Result<VisitResponse>.Fail(ErrorCodes.CoachUnavailable);
        }

        VisitTransitions.Reschedule(visit, coach.Id, startAt, endAt);
        visit.CoachStaff = coach;

        try
        {
            await visitStore.SaveChangesAsync(cancellationToken);
            await locks.CommitAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (IsUniqueViolation(ex))
        {
            return Result<VisitResponse>.Fail(ErrorCodes.VisitAlreadyScheduled);
        }

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
