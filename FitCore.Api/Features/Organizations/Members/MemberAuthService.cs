using FitCore.Api.Data.Stores.OrganizationOwner.MembersStore;
using FitCore.Api.Domain.Enums;
using FitCore.Api.Domain.Entities;
using FitCore.Api.Errors;
using FitCore.Api.Features.Organizations.Members.Activate;
using FitCore.Api.Features.Organizations.Members.Login;
using FitCore.Api.Infrastructure.Auth;

namespace FitCore.Api.Features.Organizations.Members;

public class MemberAuthService(IMemberStore memberStore, JwtTokenIssuer jwtTokenIssuer)
{
    public async Task<Result<MemberSessionResponse>> ActivateAsync(
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
            return Result<MemberSessionResponse>.Fail(ErrorCodes.InvitationInvalidOrExpired);

        var member = invite.Member;

        if (member.DeletedAt is not null)
            return Result<MemberSessionResponse>.Fail(ErrorCodes.MemberUnavailable);

        if (member.Tenant.Status != TenantStatus.Active)
            return Result<MemberSessionResponse>.Fail(ErrorCodes.OrganizationNotActive);

        member.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
        invite.UsedAt = now;

        await memberStore.SaveChangesAsync(cancellationToken);

        return Result<MemberSessionResponse>.Success(ToSession(member, "Account ready"));
    }

    public async Task<Result<MemberSessionResponse>> LoginAsync(
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
            return Result<MemberSessionResponse>.Fail(ErrorCodes.InvalidCredentials);

        if (matches.Count > 1)
            return Result<MemberSessionResponse>.Fail(ErrorCodes.EmailAmbiguousOrg);

        return Result<MemberSessionResponse>.Success(ToSession(matches[0], "Signed in"));
    }

    private MemberSessionResponse ToSession(Member member, string message) =>
        new(
            message,
            jwtTokenIssuer.CreateMemberToken(member),
            member.Tenant.Name,
            member.FirstName);
}
