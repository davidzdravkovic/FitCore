using FitCore.Api.Domain;
using FitCore.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FitCore.Api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<User> Users => Set<User>();
    public DbSet<PlatformAdmin> PlatformAdmins => Set<PlatformAdmin>();
    public DbSet<PlatformLoginToken> PlatformLoginTokens => Set<PlatformLoginToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Tenant>()
            .Property(t => t.Status)
            .HasConversion<string>();

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
    }
}
