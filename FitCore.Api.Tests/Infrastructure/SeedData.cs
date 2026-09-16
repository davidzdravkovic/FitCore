using FitCore.Api.Data;
using FitCore.Api.Domain.Members;
using FitCore.Api.Domain.Memberships;
using FitCore.Api.Domain.Plans;
using FitCore.Api.Domain.Services;
using FitCore.Api.Domain.Staffs;
using FitCore.Api.Domain.Tenants;
using Microsoft.EntityFrameworkCore;

namespace FitCore.Api.Tests.Infrastructure;

public sealed class VisitTestSeed
{
    public required Guid TenantId { get; init; }
    public required Guid MemberId { get; init; }
    public required Guid CoachStaffId { get; init; }
    public required Guid AdminStaffId { get; init; }
    public required Guid ServiceId { get; init; }
    public required Guid PlanId { get; init; }
    public required Guid MembershipId { get; init; }
}

public static class SeedData
{
    public static async Task<VisitTestSeed> SeedSessionPackScenarioAsync(
        AppDbContext db,
        int sessionsRemaining = 3,
        int planSessionCount = 3,
        CancellationToken cancellationToken = default)
    {
        if (planSessionCount <= 0)
            planSessionCount = Math.Max(sessionsRemaining, 1);

        var tenantId = Guid.NewGuid();
        var memberId = Guid.NewGuid();
        var coachId = Guid.NewGuid();
        var adminId = Guid.NewGuid();
        var serviceId = Guid.NewGuid();
        var planId = Guid.NewGuid();
        var membershipId = Guid.NewGuid();
        var now = DateTime.UtcNow;

        db.Tenants.Add(new Tenant
        {
            Id = tenantId,
            Name = $"Gym-{tenantId:N}"[..20],
            BusinessEmail = $"{tenantId:N}@test.local",
            Country = "MK",
            City = "Skopje",
            TimeZone = "Europe/Skopje",
            Currency = "EUR",
            CreatedAt = now,
            Status = TenantStatus.Active,
        });

        db.Members.Add(new Member
        {
            Id = memberId,
            TenantId = tenantId,
            FirstName = "Ada",
            LastName = "Member",
            Email = $"{memberId:N}@test.local",
            Status = MemberStatus.Active,
            CreatedAt = now,
        });

        db.Staff.Add(new Staff
        {
            Id = coachId,
            TenantId = tenantId,
            FirstName = "Coach",
            LastName = "One",
            Email = $"{coachId:N}@test.local",
            Role = StaffRole.Staff,
        });

        db.Staff.Add(new Staff
        {
            Id = adminId,
            TenantId = tenantId,
            FirstName = "Admin",
            LastName = "Owner",
            Email = $"{adminId:N}@test.local",
            Role = StaffRole.Owner,
        });

        db.Services.Add(new Service
        {
            Id = serviceId,
            TenantId = tenantId,
            Name = $"PT-{serviceId:N}"[..20],
            IsActive = true,
            CreatedAt = now,
        });

        db.MembershipPlans.Add(new MembershipPlan
        {
            Id = planId,
            TenantId = tenantId,
            ServiceId = serviceId,
            Name = $"Pack-{planId:N}"[..20],
            Price = 100m,
            EntitlementType = PlanEntitlementType.SessionPack,
            SessionCount = planSessionCount,
            IsActive = true,
            CreatedAt = now,
        });

        db.Memberships.Add(new Membership
        {
            Id = membershipId,
            TenantId = tenantId,
            MemberId = memberId,
            PlanId = planId,
            Status = MembershipStatus.Active,
            StartAt = now.AddDays(-1),
            SessionsRemaining = sessionsRemaining,
            CreatedAt = now,
        });

        await db.SaveChangesAsync(cancellationToken);

        return new VisitTestSeed
        {
            TenantId = tenantId,
            MemberId = memberId,
            CoachStaffId = coachId,
            AdminStaffId = adminId,
            ServiceId = serviceId,
            PlanId = planId,
            MembershipId = membershipId,
        };
    }

    public static async Task<int?> GetSessionsRemainingAsync(
        AppDbContext db,
        Guid membershipId,
        CancellationToken cancellationToken = default)
    {
        return await db.Memberships
            .AsNoTracking()
            .Where(m => m.Id == membershipId)
            .Select(m => m.SessionsRemaining)
            .SingleAsync(cancellationToken);
    }
}
