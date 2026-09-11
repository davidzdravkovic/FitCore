using FitCore.Api.Data.Stores.OrganizationOwner.MembersStore;
using FitCore.Api.Domain.Enums;
using FitCore.Api.Domain.Entities;
using FitCore.Api.Features.Organizations.Members.Activate;
using FitCore.Api.Features.Organizations.Members.Login;
using FitCore.Api.Infrastructure.Auth;

namespace FitCore.Api.Features.Organizations.Members;

public class MemberAuthService(IMemberStore memberStore, JwtTokenIssuer jwtTokenIssuer)
{
    public async Task<(MemberSessionResponse? Response, string? Error)> ActivateAsync(
        ActivateMemberRequest request,
        CancellationToken cancellationToken = default)
    {
        var tokenHash = InviteTokens.Hash(request.Token.Trim());
        var now = DateTime.UtcNow;

        var invite = await memberStore.FindValidInviteByTokenHashAsync(
            tokenHash,
            now,
            cancellationToken);

        if (invite is null)
            return (null, "This invitation link is invalid or has expired.");

        var member = invite.Member;

        if (member.DeletedAt is not null)
            return (null, "This member account is no longer available.");

        if (member.Tenant.Status != TenantStatus.Active)
            return (null, "This organization is not active.");

        member.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
        invite.UsedAt = now;

        await memberStore.SaveChangesAsync(cancellationToken);

        return (ToSession(member, "Account ready"), null);
    }

    public async Task<(MemberSessionResponse? Response, string? Error)> LoginAsync(
        LoginMemberRequest request,
        CancellationToken cancellationToken = default)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        var candidates = await memberStore.FindCandidatesByEmailAsync(
            email,
            cancellationToken);

        var matches = candidates
            .Where(m =>
                m.PasswordHash is not null
                && BCrypt.Net.BCrypt.Verify(request.Password, m.PasswordHash)
                && m.Tenant.Status == TenantStatus.Active)
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

    private MemberSessionResponse ToSession(Member member, string message) =>
        new(
            message,
            jwtTokenIssuer.CreateMemberToken(member),
            member.Tenant.Name,
            member.FirstName);
}
