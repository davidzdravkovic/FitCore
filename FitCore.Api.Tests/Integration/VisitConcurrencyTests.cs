using FitCore.Api.Data;
using FitCore.Api.Domain.Memberships;
using FitCore.Api.Domain.Visits;
using FitCore.Api.Errors.Business;
using FitCore.Api.Features.Organizations.Admin.Memberships;
using FitCore.Api.Features.Organizations.Admin.Memberships.Assign;
using FitCore.Api.Features.Organizations.Admin.Memberships.Cancel;
using FitCore.Api.Features.Organizations.Admin.Visits;
using FitCore.Api.Features.Organizations.Admin.Visits.Create;
using FitCore.Api.Features.Organizations.Admin.Visits.Void;
using FitCore.Api.Tests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace FitCore.Api.Tests.Integration;

/// <summary>
/// §3 concurrency (each op uses its own DI scope — DbContext is not thread-safe).
/// 1) create ‖ create same slot
/// 2) create ‖ void other visit on same pack
/// 3) create ‖ cancel membership
/// </summary>
[Collection(PostgresCollection.Name)]
public class VisitConcurrencyTests(PostgresFixture fixture)
{
    [Fact]
    public async Task Concurrent_create_same_slot_only_one_succeeds_and_one_credit()
    {
        VisitTestSeed seed;
        await using (var arrange = fixture.CreateScope())
        {
            var db = arrange.ServiceProvider.GetRequiredService<AppDbContext>();
            seed = await SeedData.SeedSessionPackScenarioAsync(db, sessionsRemaining: 3);
        }

        var start = DateTime.UtcNow.Date.AddDays(10).AddHours(9);
        var request = new CreateVisitRequest(
            seed.MembershipId,
            seed.CoachStaffId,
            start,
            start.AddHours(1));

        var task1 = CreateVisitInNewScopeAsync(seed.TenantId, request);
        var task2 = CreateVisitInNewScopeAsync(seed.TenantId, request);
        await Task.WhenAll(task1, task2);

        var results = new[] { await task1, await task2 };
        Assert.Equal(1, results.Count(r => r.Succeeded));
        Assert.Equal(1, results.Count(r => !r.Succeeded));

        var failure = results.Single(r => !r.Succeeded);
        Assert.True(
            failure.Error is ErrorCodes.MemberVisitConflict
                or ErrorCodes.VisitAlreadyScheduled
                or ErrorCodes.CoachUnavailable,
            $"Unexpected failure: {failure.Error}");

        await using var assert = fixture.CreateScope();
        var assertDb = assert.ServiceProvider.GetRequiredService<AppDbContext>();
        Assert.Equal(2, await SeedData.GetSessionsAvailableAsync(assertDb, seed.MembershipId));
        Assert.Equal(
            1,
            await assertDb.Visits.CountAsync(v =>
                v.MembershipId == seed.MembershipId && v.Status == VisitStatus.Scheduled));
    }

    [Fact]
    public async Task Concurrent_create_and_void_other_visit_keeps_credit_bucket_correct()
    {
        VisitTestSeed seed;
        Guid existingVisitId;
        await using (var arrange = fixture.CreateScope())
        {
            var db = arrange.ServiceProvider.GetRequiredService<AppDbContext>();
            var visits = arrange.ServiceProvider.GetRequiredService<VisitService>();
            seed = await SeedData.SeedSessionPackScenarioAsync(db, sessionsRemaining: 3);

            var existingStart = DateTime.UtcNow.Date.AddDays(11).AddHours(9);
            var existing = await visits.CreateAsync(
                seed.TenantId,
                new CreateVisitRequest(
                    seed.MembershipId,
                    seed.CoachStaffId,
                    existingStart,
                    existingStart.AddHours(1)));
            Assert.True(existing.Succeeded, existing.Error);
            existingVisitId = existing.Value!.Id;
            Assert.Equal(2, await SeedData.GetSessionsAvailableAsync(db, seed.MembershipId));
        }

        var newStart = DateTime.UtcNow.Date.AddDays(11).AddHours(14);
        var createTask = CreateVisitInNewScopeAsync(
            seed.TenantId,
            new CreateVisitRequest(
                seed.MembershipId,
                seed.CoachStaffId,
                newStart,
                newStart.AddHours(1)));
        var voidTask = VoidVisitInNewScopeAsync(
            seed.TenantId,
            existingVisitId,
            seed.AdminStaffId);

        await Task.WhenAll(createTask, voidTask);

        var created = await createTask;
        var voided = await voidTask;
        Assert.True(created.Succeeded, created.Error);
        Assert.True(voided.Succeeded, voided.Error);

        await using var assert = fixture.CreateScope();
        var assertDb = assert.ServiceProvider.GetRequiredService<AppDbContext>();
        Assert.Equal(2, await SeedData.GetSessionsAvailableAsync(assertDb, seed.MembershipId));
    }

    [Fact]
    public async Task Concurrent_create_and_cancel_membership_leaves_no_scheduled_visit()
    {
        VisitTestSeed seed;
        await using (var arrange = fixture.CreateScope())
        {
            var db = arrange.ServiceProvider.GetRequiredService<AppDbContext>();
            seed = await SeedData.SeedSessionPackScenarioAsync(db, sessionsRemaining: 3);
        }

        var start = DateTime.UtcNow.Date.AddDays(12).AddHours(9);
        var createTask = CreateVisitInNewScopeAsync(
            seed.TenantId,
            new CreateVisitRequest(
                seed.MembershipId,
                seed.CoachStaffId,
                start,
                start.AddHours(1)));
        var cancelTask = CancelMembershipInNewScopeAsync(
            seed.TenantId,
            seed.MembershipId,
            seed.AdminStaffId);

        await Task.WhenAll(createTask, cancelTask);

        var cancelled = await cancelTask;
        var created = await createTask;
        Assert.True(cancelled.Succeeded, cancelled.Error);

        await using var assert = fixture.CreateScope();
        var assertDb = assert.ServiceProvider.GetRequiredService<AppDbContext>();

        var membershipStatus = await assertDb.Memberships.AsNoTracking()
            .Where(m => m.Id == seed.MembershipId)
            .Select(m => m.Status)
            .SingleAsync();
        Assert.Equal(MembershipStatus.Cancelled, membershipStatus);

        Assert.Equal(
            0,
            await assertDb.Visits.CountAsync(v =>
                v.MembershipId == seed.MembershipId && v.Status == VisitStatus.Scheduled));

        if (created.Succeeded)
        {
            var visitStatus = await assertDb.Visits.AsNoTracking()
                .Where(v => v.Id == created.Value!.Id)
                .Select(v => v.Status)
                .SingleAsync();
            Assert.Equal(VisitStatus.Cancelled, visitStatus);
        }
        else
        {
            Assert.Equal(ErrorCodes.MembershipNotSchedulable, created.Error);
        }
    }

    private async Task<Result<VisitResponse>> CreateVisitInNewScopeAsync(
        Guid tenantId,
        CreateVisitRequest request)
    {
        await using var scope = fixture.CreateScope();
        var visits = scope.ServiceProvider.GetRequiredService<VisitService>();
        return await visits.CreateAsync(tenantId, request);
    }

    private async Task<Result<VisitResponse>> VoidVisitInNewScopeAsync(
        Guid tenantId,
        Guid visitId,
        Guid staffId)
    {
        await using var scope = fixture.CreateScope();
        var visits = scope.ServiceProvider.GetRequiredService<VisitService>();
        return await visits.VoidAsync(tenantId, visitId, staffId, new VoidVisitRequest());
    }

    private async Task<Result<MembershipResponse>> CancelMembershipInNewScopeAsync(
        Guid tenantId,
        Guid membershipId,
        Guid staffId)
    {
        await using var scope = fixture.CreateScope();
        var memberships = scope.ServiceProvider.GetRequiredService<MembershipService>();
        return await memberships.CancelAsync(
            tenantId,
            membershipId,
            staffId,
            new CancelMembershipRequest("AdminDecision"));
    }
}
