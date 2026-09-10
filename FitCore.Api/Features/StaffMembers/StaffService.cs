using FitCore.Api.Data;
using FitCore.Api.Domain;
using FitCore.Api.Domain.Entities;
using FitCore.Api.Features.StaffMembers.Activate;
using FitCore.Api.Features.StaffMembers.Create;
using FitCore.Api.Features.StaffMembers.Login;
using FitCore.Api.Infrastructure.App;
using FitCore.Api.Infrastructure.Auth;
using FitCore.Api.Infrastructure.Email;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace FitCore.Api.Features.StaffMembers;

public class StaffService(
    AppDbContext db,
    IEmailSender emailSender,
    JwtTokenIssuer jwtTokenIssuer,
    IOptions<AppOptions> appOptions)
{
    private static readonly TimeSpan InviteLifetime = TimeSpan.FromDays(7);

    public async Task<IReadOnlyList<StaffResponse>> ListAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        return await db.Staff
            .AsNoTracking()
            .Where(s => s.TenantId == tenantId && s.DeletedAt == null)
            .OrderBy(s => s.LastName)
            .ThenBy(s => s.FirstName)
            .Select(s => new StaffResponse(
                s.Id,
                s.FirstName,
                s.LastName,
                s.Email))
            .ToListAsync(cancellationToken);
    }

    public async Task<(StaffResponse? Response, string? Error)> CreateAsync(
        Guid tenantId,
        CreateStaffRequest request,
        CancellationToken cancellationToken = default)
    {
        var tenant = await db.Tenants
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == tenantId, cancellationToken);

        if (tenant is null)
            return (null, "Organization not found.");

        if (tenant.Status != TenantStatus.Active)
            return (null, "This organization is not active.");

        var email = request.Email.Trim().ToLowerInvariant();

        var emailTaken = await db.Staff.AnyAsync(
            s => s.TenantId == tenantId && s.Email == email && s.DeletedAt == null,
            cancellationToken);

        if (emailTaken)
            return (null, "A staff member with this email already exists.");

        var staff = new Staff
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            Email = email,
            Role = StaffRole.Staff,
            PasswordHash = null,
        };

        db.Staff.Add(staff);
        await db.SaveChangesAsync(cancellationToken);

        return (ToResponse(staff), null);
    }

    public async Task<(bool Ok, string? Error)> SoftDeleteAsync(
        Guid tenantId,
        Guid staffId,
        Guid? actingStaffId,
        CancellationToken cancellationToken = default)
    {
        if (actingStaffId is not null && actingStaffId == staffId)
            return (false, "You cannot delete your own staff account.");

        var staff = await db.Staff
            .FirstOrDefaultAsync(
                s => s.Id == staffId && s.TenantId == tenantId && s.DeletedAt == null,
                cancellationToken);

        if (staff is null)
            return (false, "Staff member not found.");

        staff.DeletedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(cancellationToken);

        return (true, null);
    }

    public async Task<(bool Ok, string? Error)> InviteAsync(
        Guid tenantId,
        Guid staffId,
        CancellationToken cancellationToken = default)
    {
        var staff = await db.Staff
            .Include(s => s.Tenant)
            .FirstOrDefaultAsync(
                s => s.Id == staffId && s.TenantId == tenantId && s.DeletedAt == null,
                cancellationToken);

        if (staff is null)
            return (false, "Staff member not found.");

        if (staff.Tenant.Status != TenantStatus.Active)
            return (false, "This organization is not active.");

        var now = DateTime.UtcNow;
        var rawToken = InviteTokens.GenerateRaw();

        var unused = await db.StaffInvites
            .Where(i => i.StaffId == staff.Id && i.UsedAt == null)
            .ToListAsync(cancellationToken);

        foreach (var invite in unused)
            invite.UsedAt = now;

        db.StaffInvites.Add(new StaffInvite
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            StaffId = staff.Id,
            TokenHash = InviteTokens.Hash(rawToken),
            ExpiresAt = now.Add(InviteLifetime),
            CreatedAt = now,
        });

        await db.SaveChangesAsync(cancellationToken);

        var baseUrl = appOptions.Value.ClientBaseUrl.TrimEnd('/');
        var activateUrl = $"{baseUrl}/staff/activate?token={Uri.EscapeDataString(rawToken)}";

        var html = $"""
            <p>{staff.Tenant.Name} invited you to sign in to FitCore.</p>
            <p><a href="{activateUrl}">Set your password</a></p>
            <p>This link expires in 7 days and can be used once.</p>
            """;

        await emailSender.SendAsync(
            staff.Email,
            "Set up your FitCore staff access",
            html,
            cancellationToken);

        return (true, null);
    }

    public async Task<(StaffSessionResponse? Response, string? Error)> ActivateAsync(
        ActivateStaffRequest request,
        CancellationToken cancellationToken = default)
    {
        var tokenHash = InviteTokens.Hash(request.Token.Trim());
        var now = DateTime.UtcNow;

        var invite = await db.StaffInvites
            .Include(i => i.Staff)
            .ThenInclude(s => s.Tenant)
            .FirstOrDefaultAsync(
                i => i.TokenHash == tokenHash && i.UsedAt == null && i.ExpiresAt > now,
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

        await db.SaveChangesAsync(cancellationToken);

        return (ToSession(staff, "Account ready"), null);
    }

    public async Task<(StaffSessionResponse? Response, string? Error)> LoginAsync(
        LoginStaffRequest request,
        CancellationToken cancellationToken = default)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        var candidates = await db.Staff
            .Include(s => s.Tenant)
            .Where(s =>
                s.Email == email
                && s.DeletedAt == null
                && s.Role == StaffRole.Staff)
            .ToListAsync(cancellationToken);

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

    private StaffSessionResponse ToSession(Staff staff, string message) =>
        new(
            message,
            staff.Role == StaffRole.Owner
                ? jwtTokenIssuer.CreateTenantOwnerToken(staff)
                : jwtTokenIssuer.CreateTenantStaffToken(staff),
            staff.Tenant.Name,
            staff.FirstName);

    private static StaffResponse ToResponse(Staff staff) =>
        new(staff.Id, staff.FirstName, staff.LastName, staff.Email);
}
