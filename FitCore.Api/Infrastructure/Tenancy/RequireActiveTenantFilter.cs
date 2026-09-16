using System.Security.Claims;
using FitCore.Api.Data.Stores.Platform;
using FitCore.Api.Domain.Tenants;
using FitCore.Api.Errors.Business;
using Microsoft.AspNetCore.Mvc.Filters;

namespace FitCore.Api.Infrastructure.Tenancy;

public sealed class RequireActiveTenantFilter(
    ITenantStore tenantStore,
    ITenantContext tenantContext) : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(
        ActionExecutingContext context,
        ActionExecutionDelegate next)
    {
        var tenantIdClaim = context.HttpContext.User.FindFirstValue("tenant_id");
        if (!Guid.TryParse(tenantIdClaim, out var tenantId))
        {
            context.Result = ErrorResults.From(ErrorCodes.MissingTenantContext);
            return;
        }

        var tenant = await tenantStore.FindByIdAsync(
            tenantId,
            context.HttpContext.RequestAborted);

        if (tenant is null)
        {
            context.Result = ErrorResults.From(ErrorCodes.OrganizationNotFound);
            return;
        }

        if (tenant.Status != TenantStatus.Active)
        {
            context.Result = ErrorResults.From(ErrorCodes.OrganizationNotActive);
            return;
        }

        tenantContext.Set(tenantId);
        await next();
    }
}
