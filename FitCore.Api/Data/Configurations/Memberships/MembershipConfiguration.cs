using FitCore.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FitCore.Api.Data.Configurations.Memberships;

public sealed class MembershipConfiguration : IEntityTypeConfiguration<Membership>
{
    public void Configure(EntityTypeBuilder<Membership> entity)
    {
        entity.ToTable("Memberships");

        entity.Property(m => m.Status).HasConversion<string>();

        entity.HasOne(m => m.Tenant)
            .WithMany(t => t.Memberships)
            .HasForeignKey(m => m.TenantId)
            .OnDelete(DeleteBehavior.Cascade);

        entity.HasOne(m => m.Member)
            .WithMany(member => member.Memberships)
            .HasForeignKey(m => m.MemberId)
            .OnDelete(DeleteBehavior.Cascade);

        entity.HasOne(m => m.Plan)
            .WithMany(p => p.Memberships)
            .HasForeignKey(m => m.PlanId)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasIndex(m => new { m.TenantId, m.MemberId, m.Status })
            .HasDatabaseName("IX_Memberships_TenantId_MemberId_Status");
    }
}
