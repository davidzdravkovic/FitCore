using FitCore.Api.Data;
using FitCore.Api.Domain.Visits;
using FitCore.Api.Errors.Business;
using FitCore.Api.Features.Organizations.Admin.Visits;
using FitCore.Api.Features.Organizations.Admin.Visits.Create;
using FitCore.Api.Features.Organizations.Admin.Visits.Reschedule;
using FitCore.Api.Features.Organizations.Admin.Visits.Resolve;
using FitCore.Api.Features.Organizations.Admin.Visits.Void;
using FitCore.Api.Features.Organizations.Staff.Schedule;
using FitCore.Api.Tests.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace FitCore.Api.Tests.Integration;

[Collection(PostgresCollection.Name)]
public class StaffScheduleIntegrationTests(PostgresFixture fixture)
{
    [Fact]
    public async Task List_returns_only_the_coach_visits()
    {
        await using var scope = fixture.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var visits = scope.ServiceProvider.GetRequiredService<VisitService>();
        var staff = scope.ServiceProvider.GetRequiredService<StaffScheduleService>();
        var seed = await SeedData.SeedSessionPackScenarioAsync(db, sessionsRemaining: 5);

        var morning = DateTime.UtcNow.Date.AddDays(6).AddHours(10);
        var afternoon = DateTime.UtcNow.Date.AddDays(6).AddHours(14);

        var mine = await visits.CreateAsync(
            seed.TenantId,
            new CreateVisitRequest(seed.MembershipId, seed.CoachStaffId, morning, morning.AddHours(1)));
        Assert.True(mine.Succeeded, mine.Error);

        var other = await visits.CreateAsync(
            seed.TenantId,
            new CreateVisitRequest(seed.MembershipId, seed.AdminStaffId, afternoon, afternoon.AddHours(1)));
        Assert.True(other.Succeeded, other.Error);

        var listed = await staff.ListAsync(
            seed.TenantId,
            seed.CoachStaffId,
            morning.Date,
            morning.Date.AddDays(1));

        Assert.Single(listed);
        Assert.Equal(mine.Value!.Id, listed[0].Id);
        Assert.Equal(seed.CoachStaffId, listed[0].CoachStaffId);
    }

    [Fact]
    public async Task Create_with_another_coach_id_fails()
    {
        await using var scope = fixture.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var staff = scope.ServiceProvider.GetRequiredService<StaffScheduleService>();
        var seed = await SeedData.SeedSessionPackScenarioAsync(db, sessionsRemaining: 3);

        var start = DateTime.UtcNow.Date.AddDays(7).AddHours(10);
        var result = await staff.CreateAsync(
            seed.TenantId,
            seed.CoachStaffId,
            new CreateVisitRequest(seed.MembershipId, seed.AdminStaffId, start, start.AddHours(1)));

        Assert.False(result.Succeeded);
        Assert.Equal(ErrorCodes.VisitCoachMismatch, result.Error);
    }

    [Fact]
    public async Task Mutations_on_another_coach_visit_are_not_found()
    {
        await using var scope = fixture.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var visits = scope.ServiceProvider.GetRequiredService<VisitService>();
        var staff = scope.ServiceProvider.GetRequiredService<StaffScheduleService>();
        var seed = await SeedData.SeedSessionPackScenarioAsync(db, sessionsRemaining: 3);

        var start = DateTime.UtcNow.Date.AddDays(8).AddHours(10);
        var other = await visits.CreateAsync(
            seed.TenantId,
            new CreateVisitRequest(seed.MembershipId, seed.AdminStaffId, start, start.AddHours(1)));
        Assert.True(other.Succeeded, other.Error);

        var resolved = await staff.ResolveAsync(
            seed.TenantId,
            seed.CoachStaffId,
            other.Value!.Id,
            new ResolveVisitRequest(VisitResolveOutcome.Completed, start.AddMinutes(5)));
        Assert.False(resolved.Succeeded);
        Assert.Equal(ErrorCodes.VisitNotFound, resolved.Error);

        var moved = await staff.RescheduleAsync(
            seed.TenantId,
            seed.CoachStaffId,
            other.Value.Id,
            new RescheduleVisitRequest(seed.CoachStaffId, start.AddDays(1), start.AddDays(1).AddHours(1)));
        Assert.False(moved.Succeeded);
        Assert.Equal(ErrorCodes.VisitNotFound, moved.Error);

        var voided = await staff.VoidAsync(
            seed.TenantId,
            seed.CoachStaffId,
            other.Value.Id,
            new VoidVisitRequest());
        Assert.False(voided.Succeeded);
        Assert.Equal(ErrorCodes.VisitNotFound, voided.Error);
    }

    [Fact]
    public async Task Create_and_void_as_coach_restores_credit()
    {
        await using var scope = fixture.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var staff = scope.ServiceProvider.GetRequiredService<StaffScheduleService>();
        var seed = await SeedData.SeedSessionPackScenarioAsync(db, sessionsRemaining: 2);

        var start = DateTime.UtcNow.Date.AddDays(9).AddHours(10);
        var created = await staff.CreateAsync(
            seed.TenantId,
            seed.CoachStaffId,
            new CreateVisitRequest(seed.MembershipId, seed.CoachStaffId, start, start.AddHours(1)));
        Assert.True(created.Succeeded, created.Error);
        Assert.Equal(1, await SeedData.GetSessionsAvailableAsync(db, seed.MembershipId));

        var voided = await staff.VoidAsync(
            seed.TenantId,
            seed.CoachStaffId,
            created.Value!.Id,
            new VoidVisitRequest("mistake"));
        Assert.True(voided.Succeeded, voided.Error);
        Assert.Equal(VisitStatus.Voided.ToString(), voided.Value!.Status);
        Assert.Equal(2, await SeedData.GetSessionsAvailableAsync(db, seed.MembershipId));
    }
}
