using FitCore.Api.Data;
using FitCore.Api.Domain.Memberships;
using FitCore.Api.Domain.Visits;
using FitCore.Api.Errors.Business;
using FitCore.Api.Features.Organizations.Admin.Memberships;
using FitCore.Api.Features.Organizations.Admin.Memberships.Cancel;
using FitCore.Api.Features.Organizations.Admin.Services;
using FitCore.Api.Features.Organizations.Admin.Visits;
using FitCore.Api.Features.Organizations.Admin.Visits.Create;
using FitCore.Api.Features.Organizations.Admin.Visits.Void;
using FitCore.Api.Tests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace FitCore.Api.Tests.Integration;

[Collection(PostgresCollection.Name)]
public class VisitMembershipIntegrationTests(PostgresFixture fixture)
{
    [Fact]
    public async Task Create_session_pack_visit_decrements_credit()
    {
        await using var scope = fixture.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var visits = scope.ServiceProvider.GetRequiredService<VisitService>();
        var seed = await SeedData.SeedSessionPackScenarioAsync(db, sessionsRemaining: 3);

        var start = DateTime.UtcNow.Date.AddDays(1).AddHours(10);
        var result = await visits.CreateAsync(
            seed.TenantId,
            new CreateVisitRequest(seed.MembershipId, seed.CoachStaffId, start, start.AddHours(1)));

        Assert.True(result.Succeeded, result.Error);
        Assert.Equal(VisitStatus.Scheduled.ToString(), result.Value!.Status);
        Assert.Equal(2, await SeedData.GetSessionsRemainingAsync(db, seed.MembershipId));
    }

    [Fact]
    public async Task Void_restores_credit_and_is_idempotent()
    {
        await using var scope = fixture.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var visits = scope.ServiceProvider.GetRequiredService<VisitService>();
        var seed = await SeedData.SeedSessionPackScenarioAsync(db, sessionsRemaining: 2);

        var start = DateTime.UtcNow.Date.AddDays(2).AddHours(10);
        var created = await visits.CreateAsync(
            seed.TenantId,
            new CreateVisitRequest(seed.MembershipId, seed.CoachStaffId, start, start.AddHours(1)));
        Assert.True(created.Succeeded, created.Error);

        var voided = await visits.VoidAsync(
            seed.TenantId,
            created.Value!.Id,
            seed.AdminStaffId,
            new VoidVisitRequest("mistake"));
        Assert.True(voided.Succeeded, voided.Error);
        Assert.Equal(VisitStatus.Voided.ToString(), voided.Value!.Status);
        Assert.False(voided.Value.ConsumedSessionCredit);
        Assert.Equal(2, await SeedData.GetSessionsRemainingAsync(db, seed.MembershipId));

        var again = await visits.VoidAsync(
            seed.TenantId,
            created.Value.Id,
            seed.AdminStaffId,
            new VoidVisitRequest());
        Assert.True(again.Succeeded, again.Error);
        Assert.Equal(2, await SeedData.GetSessionsRemainingAsync(db, seed.MembershipId));
    }

    [Fact]
    public async Task Create_with_zero_credits_fails()
    {
        await using var scope = fixture.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var visits = scope.ServiceProvider.GetRequiredService<VisitService>();
        var seed = await SeedData.SeedSessionPackScenarioAsync(
            db,
            sessionsRemaining: 0,
            planSessionCount: 3);

        var start = DateTime.UtcNow.Date.AddDays(3).AddHours(10);
        var result = await visits.CreateAsync(
            seed.TenantId,
            new CreateVisitRequest(seed.MembershipId, seed.CoachStaffId, start, start.AddHours(1)));

        Assert.False(result.Succeeded);
        Assert.Equal(ErrorCodes.NoSessionCredit, result.Error);
    }

    [Fact]
    public async Task Member_and_coach_overlap_conflict()
    {
        await using var scope = fixture.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var visits = scope.ServiceProvider.GetRequiredService<VisitService>();
        var seed = await SeedData.SeedSessionPackScenarioAsync(db, sessionsRemaining: 5);

        var start = DateTime.UtcNow.Date.AddDays(4).AddHours(10);
        var first = await visits.CreateAsync(
            seed.TenantId,
            new CreateVisitRequest(seed.MembershipId, seed.CoachStaffId, start, start.AddHours(1)));
        Assert.True(first.Succeeded, first.Error);

        var memberClash = await visits.CreateAsync(
            seed.TenantId,
            new CreateVisitRequest(
                seed.MembershipId,
                seed.CoachStaffId,
                start.AddMinutes(30),
                start.AddHours(1).AddMinutes(30)));
        Assert.False(memberClash.Succeeded);
        Assert.Equal(ErrorCodes.MemberVisitConflict, memberClash.Error);
    }

    [Fact]
    public async Task Cancel_membership_cancels_open_visit_and_blocks_create()
    {
        await using var scope = fixture.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var visits = scope.ServiceProvider.GetRequiredService<VisitService>();
        var memberships = scope.ServiceProvider.GetRequiredService<MembershipService>();
        var seed = await SeedData.SeedSessionPackScenarioAsync(db, sessionsRemaining: 4);

        var start = DateTime.UtcNow.Date.AddDays(5).AddHours(10);
        var created = await visits.CreateAsync(
            seed.TenantId,
            new CreateVisitRequest(seed.MembershipId, seed.CoachStaffId, start, start.AddHours(1)));
        Assert.True(created.Succeeded, created.Error);

        var cancelled = await memberships.CancelAsync(
            seed.TenantId,
            seed.MembershipId,
            seed.AdminStaffId,
            new CancelMembershipRequest("AdminDecision"));
        Assert.True(cancelled.Succeeded, cancelled.Error);
        Assert.Equal(MembershipStatus.Cancelled.ToString(), cancelled.Value!.Status);

        var visitStatus = await db.Visits.AsNoTracking()
            .Where(v => v.Id == created.Value!.Id)
            .Select(v => v.Status)
            .SingleAsync();
        Assert.Equal(VisitStatus.Cancelled, visitStatus);

        var after = await visits.CreateAsync(
            seed.TenantId,
            new CreateVisitRequest(
                seed.MembershipId,
                seed.CoachStaffId,
                start.AddDays(1),
                start.AddDays(1).AddHours(1)));
        Assert.False(after.Succeeded);
        Assert.Equal(ErrorCodes.MembershipNotSchedulable, after.Error);
    }

    [Fact]
    public async Task Deactivate_service_deactivates_active_plans()
    {
        await using var scope = fixture.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var services = scope.ServiceProvider.GetRequiredService<GymServiceService>();
        var seed = await SeedData.SeedSessionPackScenarioAsync(db);

        var result = await services.DeactivateAsync(seed.TenantId, seed.ServiceId);
        Assert.True(result.Succeeded, result.Error);

        var planActive = await db.MembershipPlans.AsNoTracking()
            .Where(p => p.Id == seed.PlanId)
            .Select(p => p.IsActive)
            .SingleAsync();
        var serviceActive = await db.Services.AsNoTracking()
            .Where(s => s.Id == seed.ServiceId)
            .Select(s => s.IsActive)
            .SingleAsync();

        Assert.False(serviceActive);
        Assert.False(planActive);
    }
}
