using FitCore.Api.Domain.Entities;

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
