using FitCore.Api.Data.Stores.OrganizationOwner.VisitsStore;
using FitCore.Api.Errors.Business;
using FitCore.Api.Features.Organizations.Admin.Visits;
using FitCore.Api.Features.Organizations.Admin.Visits.Create;
using FitCore.Api.Features.Organizations.Admin.Visits.Record;
using FitCore.Api.Features.Organizations.Admin.Visits.Reschedule;
using FitCore.Api.Features.Organizations.Admin.Visits.Resolve;
using FitCore.Api.Features.Organizations.Admin.Visits.Void;

namespace FitCore.Api.Features.Organizations.Staff.Schedule;

public class StaffScheduleService(VisitService visitService, IVisitStore visitStore)
{
    private const int DefaultWindowDays = 30;

    public async Task<IReadOnlyList<VisitResponse>> ListAsync(
        Guid tenantId,
        Guid coachStaffId,
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

        var visits = await visitStore.ListByCoachAsync(
            tenantId,
            coachStaffId,
            windowStart,
            windowEnd,
            cancellationToken);

        return visits.Select(VisitService.ToResponse).ToList();
    }

    public Task<Result<VisitResponse>> CreateAsync(
        Guid tenantId,
        Guid coachStaffId,
        CreateVisitRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request.CoachStaffId != coachStaffId)
            return Task.FromResult(Result<VisitResponse>.Fail(ErrorCodes.VisitCoachMismatch));

        return visitService.CreateAsync(tenantId, request, cancellationToken);
    }

    public Task<Result<VisitResponse>> RecordAsync(
        Guid tenantId,
        Guid coachStaffId,
        RecordVisitRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request.CoachStaffId != coachStaffId)
            return Task.FromResult(Result<VisitResponse>.Fail(ErrorCodes.VisitCoachMismatch));

        return visitService.RecordAsync(tenantId, request, cancellationToken);
    }

    public async Task<Result<VisitResponse>> ResolveAsync(
        Guid tenantId,
        Guid coachStaffId,
        Guid visitId,
        ResolveVisitRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!await visitStore.IsOwnedByCoachAsync(
                tenantId,
                visitId,
                coachStaffId,
                cancellationToken))
        {
            return Result<VisitResponse>.Fail(ErrorCodes.VisitNotFound);
        }

        return await visitService.ResolveScheduledAsync(
            tenantId,
            visitId,
            request,
            cancellationToken);
    }

    public async Task<Result<VisitResponse>> RescheduleAsync(
        Guid tenantId,
        Guid coachStaffId,
        Guid visitId,
        RescheduleVisitRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request.CoachStaffId != coachStaffId)
            return Result<VisitResponse>.Fail(ErrorCodes.VisitCoachMismatch);

        if (!await visitStore.IsOwnedByCoachAsync(
                tenantId,
                visitId,
                coachStaffId,
                cancellationToken))
        {
            return Result<VisitResponse>.Fail(ErrorCodes.VisitNotFound);
        }

        return await visitService.RescheduleAsync(
            tenantId,
            visitId,
            request,
            cancellationToken);
    }

    public async Task<Result<VisitResponse>> VoidAsync(
        Guid tenantId,
        Guid coachStaffId,
        Guid visitId,
        VoidVisitRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!await visitStore.IsOwnedByCoachAsync(
                tenantId,
                visitId,
                coachStaffId,
                cancellationToken))
        {
            return Result<VisitResponse>.Fail(ErrorCodes.VisitNotFound);
        }

        return await visitService.VoidAsync(
            tenantId,
            visitId,
            coachStaffId,
            request,
            cancellationToken);
    }

    private static DateTime ToUtc(DateTime value) =>
        value.Kind switch
        {
            DateTimeKind.Utc => value,
            DateTimeKind.Local => value.ToUniversalTime(),
            _ => DateTime.SpecifyKind(value, DateTimeKind.Utc),
        };
}
