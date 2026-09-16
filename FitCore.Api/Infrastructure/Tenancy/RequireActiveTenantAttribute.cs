using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace FitCore.Api.Infrastructure.Tenancy;

/// <summary>
/// Opt-in: resolve <c>tenant_id</c> claim, require the organization exists and is Active,
/// then populate <see cref="ITenantContext"/>.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class RequireActiveTenantAttribute : Attribute, IFilterFactory
{
    public bool IsReusable => false;

    public IFilterMetadata CreateInstance(IServiceProvider serviceProvider) =>
        serviceProvider.GetRequiredService<RequireActiveTenantFilter>();
}
