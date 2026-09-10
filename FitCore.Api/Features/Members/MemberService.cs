using FitCore.Api.Data;
using FitCore.Api.Domain;
using FitCore.Api.Domain.Entities;
using FitCore.Api.Features.Members.Activate;
using FitCore.Api.Features.Members.Create;
using FitCore.Api.Features.Members.Login;
using FitCore.Api.Infrastructure.App;
using FitCore.Api.Infrastructure.Auth;
using FitCore.Api.Infrastructure.Email;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace FitCore.Api.Features.Members;

public class MemberService(
    AppDbContext db,
    IEmailSender emailSender,
    JwtTokenIssuer jwtTokenIssuer,
    IOptions<AppOptions> appOptions)
{
    private static readonly TimeSpan InviteLifetime = TimeSpan.FromDays(7);

    public async Task<IReadOnlyList<MemberResponse>> ListAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        return await db.Members
            .AsNoTracking()
            .Where(m => m.TenantId == tenantId && m.DeletedAt == null)
            .OrderByDescending(m => m.CreatedAt)
            .Select(m => new MemberResponse(
                m.Id,
                m.FirstName,
                m.LastName,
                m.Email,
                m.Phone,
                m.Status.ToString(),
                m.CreatedAt))
            .ToListAsync(cancellationToken);
    }

    public async Task<(MemberResponse? Response, string? Error)> CreateAsync(
        Guid tenantId,
        CreateMemberRequest request,
        CancellationToken cancellationToken = default)
    {
        var tenant = await db.Tenants
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == tenantId, cancellationToken);

        if (tenant is null)
            return (null, "Organization not found.");

        if (tenant.Status != TenantStatus.Active)
            return (null, "This organization is not active.");

        var email = NormalizeEmail(request.Email);
        var phone = NormalizePhone(request.Phone);
        var status = Enum.Parse<MemberStatus>(request.Status.Trim(), ignoreCase: true);

        if (email is not null)
        {
            var emailTaken = await db.Members.AnyAsync(
                m => m.TenantId == tenantId && m.Email == email && m.DeletedAt == null,
                cancellationToken);

            if (emailTaken)
                return (null, "A member with this email already exists.");
        }

        if (phone is not null)
        {
            var phoneTaken = await db.Members.AnyAsync(
                m => m.TenantId == tenantId && m.Phone == phone && m.DeletedAt == null,
                cancellationToken);

            if (phoneTaken)
                return (null, "A member with this phone already exists.");
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

        db.Members.Add(member);
        await db.SaveChangesAsync(cancellationToken);

        return (ToResponse(member), null);
    }

    public async Task<(bool Ok, string? Error)> SoftDeleteAsync(
        Guid tenantId,
        Guid memberId,
        CancellationToken cancellationToken = default)
    {
        var member = await db.Members
            .FirstOrDefaultAsync(
                m => m.Id == memberId && m.TenantId == tenantId && m.DeletedAt == null,
                cancellationToken);

        if (member is null)
            return (false, "Member not found.");

        member.DeletedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(cancellationToken);

        return (true, null);
    }

    public async Task<(bool Ok, string? Error)> InviteAsync(
        Guid tenantId,
        Guid memberId,
        CancellationToken cancellationToken = default)
    {
        var member = await db.Members
            .Include(m => m.Tenant)
            .FirstOrDefaultAsync(
                m => m.Id == memberId && m.TenantId == tenantId && m.DeletedAt == null,
                cancellationToken);

        if (member is null)
            return (false, "Member not found.");

        if (member.Tenant.Status != TenantStatus.Active)
            return (false, "This organization is not active.");

        if (string.IsNullOrWhiteSpace(member.Email))
            return (false, "This member needs an email before they can be invited.");

        var now = DateTime.UtcNow;
        var rawToken = InviteTokens.GenerateRaw();

        var unused = await db.MemberInvites
            .Where(i => i.MemberId == member.Id && i.UsedAt == null)
            .ToListAsync(cancellationToken);

        foreach (var invite in unused)
            invite.UsedAt = now;

        db.MemberInvites.Add(new MemberInvite
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            MemberId = member.Id,
            TokenHash = InviteTokens.Hash(rawToken),
            ExpiresAt = now.Add(InviteLifetime),
            CreatedAt = now,
        });

        await db.SaveChangesAsync(cancellationToken);

        var baseUrl = appOptions.Value.ClientBaseUrl.TrimEnd('/');
        var activateUrl = $"{baseUrl}/member/activate?token={Uri.EscapeDataString(rawToken)}";

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

        return (true, null);
    }

    public async Task<(MemberSessionResponse? Response, string? Error)> ActivateAsync(
        ActivateMemberRequest request,
        CancellationToken cancellationToken = default)
    {
        var tokenHash = InviteTokens.Hash(request.Token.Trim());
        var now = DateTime.UtcNow;

        var invite = await db.MemberInvites
            .Include(i => i.Member)
            .ThenInclude(m => m.Tenant)
            .FirstOrDefaultAsync(
                i => i.TokenHash == tokenHash && i.UsedAt == null && i.ExpiresAt > now,
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

        await db.SaveChangesAsync(cancellationToken);

        return (ToSession(member, "Account ready"), null);
    }

    public async Task<(MemberSessionResponse? Response, string? Error)> LoginAsync(
        LoginMemberRequest request,
        CancellationToken cancellationToken = default)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        var candidates = await db.Members
            .Include(m => m.Tenant)
            .Where(m => m.Email == email && m.DeletedAt == null)
            .ToListAsync(cancellationToken);

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
