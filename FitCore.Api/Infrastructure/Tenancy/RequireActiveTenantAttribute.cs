using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace FitCore.Api.Infrastructure.Tenancy;


[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class RequireActiveTenantAttribute : Attribute, IFilterFactory
{
    public bool IsReusable => false;

    public IFilterMetadata CreateInstance(IServiceProvider serviceProvider) =>
        serviceProvider.GetRequiredService<RequireActiveTenantFilter>();
}
