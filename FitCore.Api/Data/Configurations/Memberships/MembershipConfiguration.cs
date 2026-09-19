using FitCore.Api.Domain.Memberships;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FitCore.Api.Data.Configurations.Memberships;

public sealed class MembershipConfiguration : IEntityTypeConfiguration<Membership>
{
    public void Configure(EntityTypeBuilder<Membership> entity)
    {
        entity.ToTable("Memberships", t =>
        {
            t.HasCheckConstraint(
                "CK_Memberships_SessionBuckets",
                "\"SessionTotal\" >= 0 AND \"SessionsReserved\" >= 0 AND \"SessionsBurned\" >= 0 AND \"SessionTotal\" >= (\"SessionsReserved\" + \"SessionsBurned\")");
        });

        entity.Ignore(m => m.SessionsAvailable);

        entity.Property(m => m.Status).HasConversion<string>();
        entity.Property(m => m.CancelReason).HasConversion<string>();
        entity.Property(m => m.CancelNote).HasMaxLength(500);

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

        entity.HasOne(m => m.CancelledByStaff)
            .WithMany()
            .HasForeignKey(m => m.CancelledByStaffId)
            .OnDelete(DeleteBehavior.SetNull);

        entity.HasIndex(m => new { m.TenantId, m.MemberId, m.Status })
            .HasDatabaseName("IX_Memberships_TenantId_MemberId_Status");
    }
}
