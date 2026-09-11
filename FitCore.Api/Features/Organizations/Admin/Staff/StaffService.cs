using FitCore.Api.Data.Stores.OrganizationOwner.StaffStore;
using FitCore.Api.Domain.Enums;
using FitCore.Api.Domain.Entities;
using FitCore.Api.Features.Organizations.Admin.Staff.Create;
using FitCore.Api.Infrastructure.App;
using FitCore.Api.Infrastructure.Auth;
using FitCore.Api.Infrastructure.Email;
using Microsoft.Extensions.Options;
using StaffEntity = FitCore.Api.Domain.Entities.Staff;

namespace FitCore.Api.Features.Organizations.Admin.Staff;

public class StaffService(
    IStaffStore staffStore,
    IEmailSender emailSender,
    IOptions<AppOptions> appOptions)
{
    private static readonly TimeSpan InviteLifetime = TimeSpan.FromDays(7);

    public async Task<IReadOnlyList<StaffResponse>> ListAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        var staff = await staffStore.ListByTenantAsync(tenantId, cancellationToken);

        return staff
            .Select(s => new StaffResponse(s.Id, s.FirstName, s.LastName, s.Email))
            .ToList();
    }

    public async Task<(StaffResponse? Response, string? Error)> CreateAsync(
        Guid tenantId,
        CreateStaffRequest request,
        CancellationToken cancellationToken = default)
    {
        var tenant = await staffStore.FindTenantByIdAsync(tenantId, cancellationToken);

        if (tenant is null)
            return (null, "Organization not found.");

        if (tenant.Status != TenantStatus.Active)
            return (null, "This organization is not active.");

        var email = request.Email.Trim().ToLowerInvariant();

        if (await staffStore.EmailTakenAsync(tenantId, email, cancellationToken))
            return (null, "A staff member with this email already exists.");

        var staff = new StaffEntity
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            Email = email,
            Role = StaffRole.Staff,
            PasswordHash = null,
        };

        await staffStore.AddAsync(staff);
        await staffStore.SaveChangesAsync(cancellationToken);

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

        var staff = await staffStore.FindActiveByIdAsync(tenantId, staffId, cancellationToken);

        if (staff is null)
            return (false, "Staff member not found.");

        staff.DeletedAt = DateTime.UtcNow;
        await staffStore.SaveChangesAsync(cancellationToken);

        return (true, null);
    }

    public async Task<(bool Ok, string? Error)> InviteAsync(
        Guid tenantId,
        Guid staffId,
        CancellationToken cancellationToken = default)
    {
        var staff = await staffStore.FindActiveWithTenantAsync(tenantId, staffId, cancellationToken);

        if (staff is null)
            return (false, "Staff member not found.");

        if (staff.Tenant.Status != TenantStatus.Active)
            return (false, "This organization is not active.");

        var now = DateTime.UtcNow;
        var rawToken = InviteTokens.GenerateRaw();

        await staffStore.InvalidateUnusedInvitesAsync(tenantId, staff.Id, now, cancellationToken);
        await staffStore.AddInviteAsync(new StaffInvite
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            StaffId = staff.Id,
            TokenHash = InviteTokens.Hash(rawToken),
            ExpiresAt = now.Add(InviteLifetime),
            CreatedAt = now,
        });
        await staffStore.SaveChangesAsync(cancellationToken);

        var activateUrl = ClientLinks.Activate(
            appOptions.Value.ClientBaseUrl,
            "staff/activate",
            rawToken);

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

    private static StaffResponse ToResponse(StaffEntity staff) =>
        new(staff.Id, staff.FirstName, staff.LastName, staff.Email);
}
