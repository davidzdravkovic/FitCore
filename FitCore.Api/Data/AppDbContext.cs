using FitCore.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FitCore.Api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<Staff> Staff => Set<Staff>();
    public DbSet<Member> Members => Set<Member>();
    public DbSet<PlatformAdmin> PlatformAdmins => Set<PlatformAdmin>();
    public DbSet<PlatformLoginToken> PlatformLoginTokens => Set<PlatformLoginToken>();
    public DbSet<Invitation> Invitations => Set<Invitation>();
    public DbSet<StaffInvite> StaffInvites => Set<StaffInvite>();
    public DbSet<MemberInvite> MemberInvites => Set<MemberInvite>();
    public DbSet<Service> Services => Set<Service>();
    public DbSet<MembershipPlan> MembershipPlans => Set<MembershipPlan>();
    public DbSet<Membership> Memberships => Set<Membership>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
