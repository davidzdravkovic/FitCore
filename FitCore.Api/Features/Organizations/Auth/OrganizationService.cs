using System.Security.Cryptography;
using System.Text;
using FitCore.Api.Data;
using FitCore.Api.Domain;
using FitCore.Api.Domain.Entities;
using FitCore.Api.Features.Organizations.Login;
using FitCore.Api.Features.Organizations.Register;
using FitCore.Api.Infrastructure.Auth;
using Microsoft.EntityFrameworkCore;

namespace FitCore.Api.Features.Organizations;

public class OrganizationService(AppDbContext db, JwtTokenIssuer jwtTokenIssuer)
{
    public async Task<(RegisterOrganizationResponse? Response, string? Error)> RegisterAsync(
        RegisterOrganizationRequest request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.InvitationToken))
            return (null, "Invitation token is required.");

        var tokenHash = HashToken(request.InvitationToken.Trim());
        var now = DateTime.UtcNow;

        var invitation = await db.Invitations
            .FirstOrDefaultAsync(
                i => i.TokenHash == tokenHash && i.UsedAt == null && i.ExpiresAt > now,
                cancellationToken);

        if (invitation is null)
            return (null, "This invitation link is invalid or has expired.");

        var ownerEmail = request.OwnerEmail.Trim().ToLowerInvariant();
        var businessEmail = request.BusinessEmail.Trim().ToLowerInvariant();


        var tenant = new Tenant
        {
            Id = Guid.NewGuid(),
            Name = request.OrganizationName.Trim(),
            BusinessEmail = businessEmail,
            Country = request.Country.Trim(),
            City = request.City.Trim(),
            TimeZone = request.TimeZone.Trim(),
            CreatedAt = now,
            Status = TenantStatus.Active,
        };

        var owner = new Staff
        {
            Id = Guid.NewGuid(),
            TenantId = tenant.Id,
            FirstName = request.OwnerFirstName.Trim(),
            LastName = request.OwnerLastName.Trim(),
            Email = ownerEmail,
            Role = StaffRole.Owner,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.OwnerPassword),
        };

        invitation.UsedAt = now;
        db.Tenants.Add(tenant);
        db.Staff.Add(owner);

        await db.SaveChangesAsync(cancellationToken);

        var accessToken = jwtTokenIssuer.CreateTenantOwnerToken(owner);

        return (
            new RegisterOrganizationResponse(
                "Organization created",
                accessToken,
                tenant.Name,
                owner.FirstName),
            null);
    }

    public async Task<(LoginOrganizationResponse? Response, string? Error)> LoginAsync(
        LoginOrganizationRequest request,
        CancellationToken cancellationToken = default)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        var candidates = await db.Staff
            .Include(s => s.Tenant)
            .Where(s =>
                s.Email == email
                && s.DeletedAt == null
                && s.Role == StaffRole.Owner)
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

        var staff = matches[0];
        var accessToken = jwtTokenIssuer.CreateTenantOwnerToken(staff);

        return (
            new LoginOrganizationResponse(
                "Signed in",
                accessToken,
                staff.Tenant.Name,
                staff.FirstName),
            null);
    }

    private static string HashToken(string rawToken)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(rawToken));
        return Convert.ToHexString(bytes);
    }
}
