using FitCore.Api.Domain;
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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Tenant>()
            .Property(t => t.Status)
            .HasConversion<string>();

        modelBuilder.Entity<Staff>(entity =>
        {
            // Email uniqueness is per tenant: the same address may be staff at multiple gyms.
            entity.ToTable("Staff");

            entity.Property(s => s.Role).HasConversion<string>();

            entity.HasIndex(s => new { s.TenantId, s.Email })
                .IsUnique()
                .HasFilter("\"DeletedAt\" IS NULL")
                .HasDatabaseName("IX_Staff_TenantId_Email");

            entity.HasOne(s => s.Tenant)
                .WithMany(t => t.Staff)
                .HasForeignKey(s => s.TenantId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Member>(entity =>
        {
            entity.Property(m => m.Status).HasConversion<string>();

            entity.ToTable(t => t.HasCheckConstraint(
                "CK_Members_EmailOrPhone",
                "(\"Email\" IS NOT NULL AND btrim(\"Email\") <> '') OR (\"Phone\" IS NOT NULL AND btrim(\"Phone\") <> '')"));

            entity.HasOne(m => m.Tenant)
                .WithMany(t => t.Members)
                .HasForeignKey(m => m.TenantId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(m => new { m.TenantId, m.Email })
                .IsUnique()
                .HasFilter("\"Email\" IS NOT NULL AND \"DeletedAt\" IS NULL")
                .HasDatabaseName("IX_Members_TenantId_Email");

            entity.HasIndex(m => new { m.TenantId, m.Phone })
                .IsUnique()
                .HasFilter("\"Phone\" IS NOT NULL AND \"DeletedAt\" IS NULL")
                .HasDatabaseName("IX_Members_TenantId_Phone");
        });

        modelBuilder.Entity<PlatformAdmin>()
            .HasIndex(a => a.Email)
            .IsUnique();

        modelBuilder.Entity<PlatformLoginToken>(entity =>
        {
            entity.HasIndex(t => t.TokenHash).IsUnique();

            entity.HasIndex(t => t.PlatformAdminId)
                .IsUnique()
                .HasFilter("\"UsedAt\" IS NULL")
                .HasDatabaseName("IX_PlatformLoginTokens_OneUnusedPerAdmin");

            entity.HasOne(t => t.PlatformAdmin)
                .WithMany()
                .HasForeignKey(t => t.PlatformAdminId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Invitation>(entity =>
        {
            entity.Property(i => i.Plan).HasConversion<string>();

            entity.HasIndex(i => i.TokenHash).IsUnique();

            entity.HasIndex(i => i.Email)
                .IsUnique()
                .HasFilter("\"UsedAt\" IS NULL")
                .HasDatabaseName("IX_Invitations_OneUnusedPerEmail");
        });

        modelBuilder.Entity<StaffInvite>(entity =>
        {
            entity.HasIndex(i => i.TokenHash).IsUnique();

            entity.HasIndex(i => i.StaffId)
                .IsUnique()
                .HasFilter("\"UsedAt\" IS NULL")
                .HasDatabaseName("IX_StaffInvites_OneUnusedPerStaff");

            entity.HasOne(i => i.Staff)
                .WithMany()
                .HasForeignKey(i => i.StaffId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(i => i.Tenant)
                .WithMany()
                .HasForeignKey(i => i.TenantId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<MemberInvite>(entity =>
        {
            entity.HasIndex(i => i.TokenHash).IsUnique();

            entity.HasIndex(i => i.MemberId)
                .IsUnique()
                .HasFilter("\"UsedAt\" IS NULL")
                .HasDatabaseName("IX_MemberInvites_OneUnusedPerMember");

            entity.HasOne(i => i.Member)
                .WithMany()
                .HasForeignKey(i => i.MemberId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(i => i.Tenant)
                .WithMany()
                .HasForeignKey(i => i.TenantId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
