using FitCore.Api.Data.Stores.OrganizationOwner.MembersStore;
using FitCore.Api.Domain.Members;
using FitCore.Api.Domain.Tenants;
using FitCore.Api.Errors.Business;
using FitCore.Api.Features.Organizations.Admin.Members.Cancel;
using FitCore.Api.Features.Organizations.Admin.Members.Create;
using FitCore.Api.Infrastructure.App;
using FitCore.Api.Infrastructure.Auth;
using FitCore.Api.Infrastructure.Email;

namespace FitCore.Api.Features.Organizations.Admin.Members;

public class MemberService(
    IMemberStore memberStore,
    IEmailSender emailSender,
    IClientLinks clientLinks)
{
    private static readonly TimeSpan InviteLifetime = TimeSpan.FromDays(7);

    public async Task<IReadOnlyList<MemberResponse>> ListAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        var members = await memberStore.ListByTenantAsync(tenantId, cancellationToken);

        return members
            .Select(m => new MemberResponse(
                m.Id,
                m.FirstName,
                m.LastName,
                m.Email,
                m.Phone,
                m.Status.ToString(),
                m.CreatedAt))
            .ToList();
    }

    public Task<Result<MemberResponse>> CreateAsync(
        Guid tenantId,
        CreateMemberRequest request,
        CancellationToken cancellationToken = default) =>
        AddAsync(tenantId, request, MemberStatus.Lead, cancellationToken);

    public Task<Result<MemberResponse>> ImportAsync(
        Guid tenantId,
        CreateMemberRequest request,
        CancellationToken cancellationToken = default) =>
        AddAsync(tenantId, request, MemberStatus.Paused, cancellationToken);

    private async Task<Result<MemberResponse>> AddAsync(
        Guid tenantId,
        CreateMemberRequest request,
        MemberStatus status,
        CancellationToken cancellationToken)
    {
        var tenant = await memberStore.FindTenantByIdAsync(tenantId, cancellationToken);

        if (tenant is null)
            return Result<MemberResponse>.Fail(ErrorCodes.OrganizationNotFound);

        if (tenant.Status != TenantStatus.Active)
            return Result<MemberResponse>.Fail(ErrorCodes.OrganizationNotActive);

        var email = NormalizeEmail(request.Email);
        var phone = NormalizePhone(request.Phone);

        if (email is not null)
        {
            if (await memberStore.EmailTakenAsync(tenantId, email, cancellationToken))
                return Result<MemberResponse>.Fail(ErrorCodes.MemberEmailTaken);
        }

        if (phone is not null)
        {
            if (await memberStore.PhoneTakenAsync(tenantId, phone, cancellationToken))
                return Result<MemberResponse>.Fail(ErrorCodes.MemberPhoneTaken);
        }

        var member = new Member
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            Email = email,
            Phone = phone,
            Status = status,
            CreatedAt = DateTime.UtcNow,
        };

        await memberStore.AddAsync(member);
        await memberStore.SaveChangesAsync(cancellationToken);

        return Result<MemberResponse>.Success(ToResponse(member));
    }

    public async Task<Result> CancelAsync(
        Guid tenantId,
        Guid memberId,
        CancellationToken cancellationToken = default)
    {
        var member = await memberStore.FindActiveByIdAsync(tenantId, memberId, cancellationToken);

        if (member is null)
            return Result.Fail(ErrorCodes.MemberNotFound);

        var unresolved = await memberStore.ListUnresolvedMembershipsForMemberAsync(
            tenantId,
            memberId,
            cancellationToken);

        if (unresolved.Count > 0)
        {
            var details = unresolved
                .Select(m => new UnresolvedMembershipResponse(
                    m.Id,
                    m.PlanId,
                    m.Plan.Name,
                    m.Status.ToString(),
                    m.StartAt,
                    m.EndAt,
                    m.SessionsRemaining))
                .ToList();

            return Result.Fail(ErrorCodes.MemberHasUnresolvedMemberships, details);
        }

        member.Status = MemberStatus.Cancelled;
        await memberStore.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result> InviteAsync(
        Guid tenantId,
        Guid memberId,
        CancellationToken cancellationToken = default)
    {
        var member = await memberStore.FindActiveWithTenantAsync(
            tenantId,
            memberId,
            cancellationToken);

        if (member is null)
            return Result.Fail(ErrorCodes.MemberNotFound);

        if (member.Tenant.Status != TenantStatus.Active)
            return Result.Fail(ErrorCodes.OrganizationNotActive);

        if (string.IsNullOrWhiteSpace(member.Email))
            return Result.Fail(ErrorCodes.MemberEmailRequired);

        var now = DateTime.UtcNow;
        var rawToken = InviteTokens.GenerateRaw();

        await memberStore.InvalidateUnusedInvitesAsync(tenantId, member.Id, now, cancellationToken);
        await memberStore.AddInviteAsync(new MemberInvite
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            MemberId = member.Id,
            TokenHash = InviteTokens.Hash(rawToken),
            ExpiresAt = now.Add(InviteLifetime),
            CreatedAt = now,
        });
        await memberStore.SaveChangesAsync(cancellationToken);

        var activateUrl = clientLinks.Activate("member/activate", rawToken);

        var html = $"""
            <p>{member.Tenant.Name} invited you to set up your FitCore account.</p>
            <p><a href="{activateUrl}">Set your password</a></p>
            <p>This link expires in 7 days and can be used once.</p>
            """;

        await emailSender.SendAsync(
            member.Email!,
            "Set up your FitCore account",
            html,
            cancellationToken);

        return Result.Success();
    }

    private static MemberResponse ToResponse(Member member) =>
        new(
            member.Id,
            member.FirstName,
            member.LastName,
            member.Email,
            member.Phone,
            member.Status.ToString(),
            member.CreatedAt);

    private static string? NormalizeEmail(string? value)
    {
        var trimmed = value?.Trim();
        return string.IsNullOrEmpty(trimmed) ? null : trimmed.ToLowerInvariant();
    }

    private static string? NormalizePhone(string? value)
    {
        var trimmed = value?.Trim();
        return string.IsNullOrEmpty(trimmed) ? null : trimmed;
    }
}
