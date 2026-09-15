using FitCore.Api.Domain.Platform;
using FitCore.Api.Domain.Staffs;
using FitCore.Api.Domain.Tenants;

namespace FitCore.Api.Data.Stores.OrganizationOwner.AuthStore;

public interface IOrganizationAuthStore
{
    Task<Invitation?> FindValidInvitationByTokenHashAsync(
        string tokenHash,
        DateTime utcNow,
        CancellationToken cancellationToken = default);

    Task MarkInvitationUsedAsync(
        Invitation invitation,
        DateTime usedAt);

    Task AddTenantAsync(Tenant tenant);

    Task AddOwnerAsync(Staff owner);

    Task<IReadOnlyList<Staff>> FindOwnerCandidatesByEmailAsync(
        string normalizedEmail,
        CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
