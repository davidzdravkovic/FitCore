using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using FitCore.Api.Errors.Business;
using FitCore.Api.Features.Organizations.Admin.Visits;
using FitCore.Api.Features.Organizations.Admin.Visits.Create;
using FitCore.Api.Features.Organizations.Admin.Visits.Record;
using FitCore.Api.Features.Organizations.Admin.Visits.Reschedule;
using FitCore.Api.Features.Organizations.Admin.Visits.Resolve;
using FitCore.Api.Features.Organizations.Admin.Visits.Void;
using FitCore.Api.Infrastructure.Tenancy;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FitCore.Api.Features.Organizations.Staff.Schedule;

[ApiController]
[Route("api/staff/me/visits")]
[Authorize(Roles = "TenantStaff")]
[RequireActiveTenant]
public class StaffScheduleController(
    StaffScheduleService staffScheduleService,
    ITenantContext tenantContext) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<VisitResponse>>> List(
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        CancellationToken cancellationToken)
    {
        if (!TryStaffId(out var coachId))
            return ErrorResults.From(ErrorCodes.MissingTenantContext);

        var visits = await staffScheduleService.ListAsync(
            tenantContext.TenantId,
            coachId,
            from,
            to,
            cancellationToken);
        return Ok(visits);
    }

    [HttpPost]
    public async Task<ActionResult<VisitResponse>> Create(
        [FromBody] CreateVisitRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryStaffId(out var coachId))
            return ErrorResults.From(ErrorCodes.MissingTenantContext);

        var result = await staffScheduleService.CreateAsync(
            tenantContext.TenantId,
            coachId,
            request,
            cancellationToken);

        if (!result.Succeeded)
            return ErrorResults.From(result.Error!);

        return Ok(result.Value);
    }

    [HttpPost("record")]
    public async Task<ActionResult<VisitResponse>> Record(
        [FromBody] RecordVisitRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryStaffId(out var coachId))
            return ErrorResults.From(ErrorCodes.MissingTenantContext);

        var result = await staffScheduleService.RecordAsync(
            tenantContext.TenantId,
            coachId,
            request,
            cancellationToken);

        if (!result.Succeeded)
            return ErrorResults.From(result.Error!);

        return Ok(result.Value);
    }

    [HttpPost("{id:guid}/resolve")]
    public async Task<ActionResult<VisitResponse>> Resolve(
        Guid id,
        [FromBody] ResolveVisitRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryStaffId(out var coachId))
            return ErrorResults.From(ErrorCodes.MissingTenantContext);

        var result = await staffScheduleService.ResolveAsync(
            tenantContext.TenantId,
            coachId,
            id,
            request,
            cancellationToken);

        if (!result.Succeeded)
            return ErrorResults.From(result.Error!);

        return Ok(result.Value);
    }

    [HttpPost("{id:guid}/reschedule")]
    public async Task<ActionResult<VisitResponse>> Reschedule(
        Guid id,
        [FromBody] RescheduleVisitRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryStaffId(out var coachId))
            return ErrorResults.From(ErrorCodes.MissingTenantContext);

        var result = await staffScheduleService.RescheduleAsync(
            tenantContext.TenantId,
            coachId,
            id,
            request,
            cancellationToken);

        if (!result.Succeeded)
            return ErrorResults.From(result.Error!);

        return Ok(result.Value);
    }

    [HttpPost("{id:guid}/void")]
    public async Task<ActionResult<VisitResponse>> Void(
        Guid id,
        [FromBody] VoidVisitRequest? request,
        CancellationToken cancellationToken)
    {
        if (!TryStaffId(out var coachId))
            return ErrorResults.From(ErrorCodes.MissingTenantContext);

        var result = await staffScheduleService.VoidAsync(
            tenantContext.TenantId,
            coachId,
            id,
            request ?? new VoidVisitRequest(),
            cancellationToken);

        if (!result.Succeeded)
            return ErrorResults.From(result.Error!);

        return Ok(result.Value);
    }

    private bool TryStaffId(out Guid staffId)
    {
        var sub = User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(sub, out staffId);
    }
}
