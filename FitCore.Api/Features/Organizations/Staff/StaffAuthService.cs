using FitCore.Api.Data.Stores.OrganizationOwner.StaffStore;
using FitCore.Api.Domain.Enums;
using FitCore.Api.Domain.Entities;
using FitCore.Api.Features.Organizations.Staff.Activate;
using FitCore.Api.Features.Organizations.Staff.Login;
using FitCore.Api.Infrastructure.Auth;
using StaffEntity = FitCore.Api.Domain.Entities.Staff;

namespace FitCore.Api.Features.Organizations.Staff;

public class StaffAuthService(IStaffStore staffStore, JwtTokenIssuer jwtTokenIssuer)
{
    public async Task<(StaffSessionResponse? Response, string? Error)> ActivateAsync(
        ActivateStaffRequest request,
        CancellationToken cancellationToken = default)
    {
        var tokenHash = InviteTokens.Hash(request.Token.Trim());
        var now = DateTime.UtcNow;

        var invite = await staffStore.FindValidInviteByTokenHashAsync(
            tokenHash,
            now,
            cancellationToken);

        if (invite is null)
            return (null, "This invitation link is invalid or has expired.");

        var staff = invite.Staff;

        if (staff.DeletedAt is not null)
            return (null, "This staff account is no longer available.");

        if (staff.Tenant.Status != TenantStatus.Active)
            return (null, "This organization is not active.");

        staff.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
        invite.UsedAt = now;

        await staffStore.SaveChangesAsync(cancellationToken);

        return (ToSession(staff, "Account ready"), null);
    }

    public async Task<(StaffSessionResponse? Response, string? Error)> LoginAsync(
        LoginStaffRequest request,
        CancellationToken cancellationToken = default)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        var candidates = await staffStore.FindStaffCandidatesByEmailAsync(
            email,
            cancellationToken);

        var matches = candidates
            .Where(s =>
                s.PasswordHash is not null
                && BCrypt.Net.BCrypt.Verify(request.Password, s.PasswordHash)
                && s.Tenant.Status == TenantStatus.Active)
            .ToList();

        if (matches.Count == 0)
            return (null, "Invalid email or password");

        if (matches.Count > 1)
        {
            return (
                null,
                "This email belongs to more than one organization. Choose an organization to continue.");
        }

        return (ToSession(matches[0], "Signed in"), null);
    }

    private StaffSessionResponse ToSession(StaffEntity staff, string message) =>
        new(
            message,
            jwtTokenIssuer.CreateTenantStaffToken(staff),
            staff.Tenant.Name,
            staff.FirstName);
}
