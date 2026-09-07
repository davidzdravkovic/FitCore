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

        var ownerExists = await db.Users
            .AnyAsync(u => u.Email == ownerEmail, cancellationToken);

        if (ownerExists)
            return (null, "An account with this owner email already exists.");

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

        var owner = new User
        {
            Id = Guid.NewGuid(),
            TenantId = tenant.Id,
            FirstName = request.OwnerFirstName.Trim(),
            LastName = request.OwnerLastName.Trim(),
            Email = ownerEmail,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.OwnerPassword),
        };

        invitation.UsedAt = now;
        db.Tenants.Add(tenant);
        db.Users.Add(owner);

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

        var user = await db.Users
            .Include(u => u.Tenant)
            .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);

        if (user is null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            return (null, "Invalid email or password");

        if (user.Tenant.Status != TenantStatus.Active)
            return (null, "This organization is not active.");

        var accessToken = jwtTokenIssuer.CreateTenantOwnerToken(user);

        return (
            new LoginOrganizationResponse(
                "Signed in",
                accessToken,
                user.Tenant.Name,
                user.FirstName),
            null);
    }

    private static string HashToken(string rawToken)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(rawToken));
        return Convert.ToHexString(bytes);
    }
}
