using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using FitCore.Api.Errors.Business;
using FitCore.Api.Features.Organizations.Admin.Visits.Create;
using FitCore.Api.Features.Organizations.Admin.Visits.Record;
using FitCore.Api.Features.Organizations.Admin.Visits.Reschedule;
using FitCore.Api.Features.Organizations.Admin.Visits.Resolve;
using FitCore.Api.Features.Organizations.Admin.Visits.Void;
using FitCore.Api.Infrastructure.Tenancy;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FitCore.Api.Features.Organizations.Admin.Visits;

[ApiController]
[Route("api/visits")]
[Authorize(Roles = "TenantOwner")]
[RequireActiveTenant]
public class VisitsController(VisitService visitService, ITenantContext tenantContext) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<VisitResponse>>> List(
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        CancellationToken cancellationToken)
    {
        var visits = await visitService.ListAsync(
            tenantContext.TenantId,
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
        var result = await visitService.CreateAsync(
            tenantContext.TenantId,
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
        var result = await visitService.RecordAsync(
            tenantContext.TenantId,
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
        var result = await visitService.ResolveScheduledAsync(
            tenantContext.TenantId,
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
        var result = await visitService.RescheduleAsync(
            tenantContext.TenantId,
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
        var sub = User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(sub, out var staffId))
            return ErrorResults.From(ErrorCodes.MissingTenantContext);

        var result = await visitService.VoidAsync(
            tenantContext.TenantId,
            id,
            staffId,
            request ?? new VoidVisitRequest(),
            cancellationToken);

        if (!result.Succeeded)
            return ErrorResults.From(result.Error!);

        return Ok(result.Value);
    }
}
