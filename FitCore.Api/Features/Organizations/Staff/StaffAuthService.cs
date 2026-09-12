using FitCore.Api.Data.Stores.OrganizationOwner.StaffStore;
using FitCore.Api.Domain.Enums;
using FitCore.Api.Domain.Entities;
using FitCore.Api.Errors.Business;
using FitCore.Api.Features.Organizations.Staff.Activate;
using FitCore.Api.Features.Organizations.Staff.Login;
using FitCore.Api.Infrastructure.Auth;
using StaffEntity = FitCore.Api.Domain.Entities.Staff;

namespace FitCore.Api.Features.Organizations.Staff;

public class StaffAuthService(IStaffStore staffStore, JwtTokenIssuer jwtTokenIssuer)
{
    public async Task<Result<StaffSessionResponse>> ActivateAsync(
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
            return Result<StaffSessionResponse>.Fail(ErrorCodes.InvitationInvalidOrExpired);

        var staff = invite.Staff;

        if (staff.DeletedAt is not null)
            return Result<StaffSessionResponse>.Fail(ErrorCodes.StaffUnavailable);

        if (staff.Tenant.Status != TenantStatus.Active)
            return Result<StaffSessionResponse>.Fail(ErrorCodes.OrganizationNotActive);

        staff.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
        invite.UsedAt = now;

        await staffStore.SaveChangesAsync(cancellationToken);

        return Result<StaffSessionResponse>.Success(ToSession(staff, "Account ready"));
    }

    public async Task<Result<StaffSessionResponse>> LoginAsync(
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
            return Result<StaffSessionResponse>.Fail(ErrorCodes.InvalidCredentials);

        if (matches.Count > 1)
            return Result<StaffSessionResponse>.Fail(ErrorCodes.EmailAmbiguousOrg);

        return Result<StaffSessionResponse>.Success(ToSession(matches[0], "Signed in"));
    }

    private StaffSessionResponse ToSession(StaffEntity staff, string message) =>
        new(
            message,
            jwtTokenIssuer.CreateTenantStaffToken(staff),
            staff.Tenant.Name,
            staff.FirstName);
}
