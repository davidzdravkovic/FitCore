using FitCore.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FitCore.Api.Data.Configurations.Memberships;

public sealed class MembershipPlanConfiguration : IEntityTypeConfiguration<MembershipPlan>
{
    public void Configure(EntityTypeBuilder<MembershipPlan> entity)
    {
        entity.ToTable("MembershipPlans", t => t.HasCheckConstraint(
            "CK_MembershipPlans_Entitlement",
            "(\"EntitlementType\" = 'SessionPack' AND \"SessionCount\" IS NOT NULL AND \"SessionCount\" > 0) OR (\"EntitlementType\" = 'TimePeriod' AND \"DurationDays\" IS NOT NULL AND \"DurationDays\" > 0)"));

        entity.Property(p => p.EntitlementType).HasConversion<string>();
        entity.Property(p => p.Price).HasPrecision(18, 2);
        entity.Property(p => p.Currency).HasMaxLength(3);

        entity.HasOne(p => p.Tenant)
            .WithMany(t => t.MembershipPlans)
            .HasForeignKey(p => p.TenantId)
            .OnDelete(DeleteBehavior.Cascade);

        entity.HasOne(p => p.Service)
            .WithMany(s => s.Plans)
            .HasForeignKey(p => p.ServiceId)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasIndex(p => new { p.TenantId, p.Name })
            .IsUnique()
            .HasDatabaseName("IX_MembershipPlans_TenantId_Name");
    }
}
