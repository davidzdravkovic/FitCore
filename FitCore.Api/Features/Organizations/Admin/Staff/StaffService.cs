using FitCore.Api.Data.Stores.OrganizationOwner.StaffStore;
using FitCore.Api.Domain.Enums;
using FitCore.Api.Domain.Entities;
using FitCore.Api.Errors;
using FitCore.Api.Features.Organizations.Admin.Staff.Create;
using FitCore.Api.Infrastructure.App;
using FitCore.Api.Infrastructure.Auth;
using FitCore.Api.Infrastructure.Email;
using StaffEntity = FitCore.Api.Domain.Entities.Staff;

namespace FitCore.Api.Features.Organizations.Admin.Staff;

public class StaffService(
    IStaffStore staffStore,
    IEmailSender emailSender,
    IClientLinks clientLinks)
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

    public async Task<Result<StaffResponse>> CreateAsync(
        Guid tenantId,
        CreateStaffRequest request,
        CancellationToken cancellationToken = default)
    {
        var tenant = await staffStore.FindTenantByIdAsync(tenantId, cancellationToken);

        if (tenant is null)
            return Result<StaffResponse>.Fail(ErrorCodes.OrganizationNotFound);

        if (tenant.Status != TenantStatus.Active)
            return Result<StaffResponse>.Fail(ErrorCodes.OrganizationNotActive);

        var email = request.Email.Trim().ToLowerInvariant();

        if (await staffStore.EmailTakenAsync(tenantId, email, cancellationToken))
            return Result<StaffResponse>.Fail(ErrorCodes.StaffEmailTaken);

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

        return Result<StaffResponse>.Success(ToResponse(staff));
    }

    public async Task<Result> SoftDeleteAsync(
        Guid tenantId,
        Guid staffId,
        Guid? actingStaffId,
        CancellationToken cancellationToken = default)
    {
        if (actingStaffId is not null && actingStaffId == staffId)
            return Result.Fail(ErrorCodes.CannotDeleteSelf);

        var staff = await staffStore.FindActiveByIdAsync(tenantId, staffId, cancellationToken);

        if (staff is null)
            return Result.Fail(ErrorCodes.StaffNotFound);

        staff.DeletedAt = DateTime.UtcNow;
        await staffStore.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result> InviteAsync(
        Guid tenantId,
        Guid staffId,
        CancellationToken cancellationToken = default)
    {
        var staff = await staffStore.FindActiveWithTenantAsync(tenantId, staffId, cancellationToken);

        if (staff is null)
            return Result.Fail(ErrorCodes.StaffNotFound);

        if (staff.Tenant.Status != TenantStatus.Active)
            return Result.Fail(ErrorCodes.OrganizationNotActive);

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

        var activateUrl = clientLinks.Activate("staff/activate", rawToken);

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

        return Result.Success();
    }

    private static StaffResponse ToResponse(StaffEntity staff) =>
        new(staff.Id, staff.FirstName, staff.LastName, staff.Email);
}
