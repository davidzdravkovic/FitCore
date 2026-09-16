using FitCore.Api.Domain.Visits;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FitCore.Api.Data.Configurations.Visits;

public sealed class VisitConfiguration : IEntityTypeConfiguration<Visit>
{
    public void Configure(EntityTypeBuilder<Visit> entity)
    {
        entity.ToTable("Visits");

        entity.Property(v => v.Status).HasConversion<string>();
        entity.Property(v => v.VoidNote).HasMaxLength(500);

        entity.ToTable(t => t.HasCheckConstraint(
            "CK_Visits_EndAfterStart",
            "\"EndAt\" > \"StartAt\""));

        entity.HasOne(v => v.Tenant)
            .WithMany(t => t.Visits)
            .HasForeignKey(v => v.TenantId)
            .OnDelete(DeleteBehavior.Cascade);

        entity.HasOne(v => v.Membership)
            .WithMany()
            .HasForeignKey(v => v.MembershipId)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasOne(v => v.Member)
            .WithMany()
            .HasForeignKey(v => v.MemberId)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasOne(v => v.CoachStaff)
            .WithMany()
            .HasForeignKey(v => v.CoachStaffId)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasOne(v => v.Service)
            .WithMany()
            .HasForeignKey(v => v.ServiceId)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasOne(v => v.VoidedByStaff)
            .WithMany()
            .HasForeignKey(v => v.VoidedByStaffId)
            .OnDelete(DeleteBehavior.SetNull);

        entity.HasIndex(v => new { v.TenantId, v.StartAt })
            .HasDatabaseName("IX_Visits_TenantId_StartAt");

        entity.HasIndex(v => new { v.TenantId, v.CoachStaffId, v.StartAt })
            .HasDatabaseName("IX_Visits_TenantId_CoachStaffId_StartAt");

        entity.HasIndex(v => new { v.TenantId, v.MemberId, v.StartAt })
            .HasDatabaseName("IX_Visits_TenantId_MemberId_StartAt");

        entity.HasIndex(v => new { v.TenantId, v.MembershipId, v.Status })
            .HasDatabaseName("IX_Visits_TenantId_MembershipId_Status");

        entity.HasIndex(v => new
            {
                v.TenantId,
                v.MembershipId,
                v.CoachStaffId,
                v.StartAt,
                v.EndAt,
            })
            .IsUnique()
            .HasFilter("\"Status\" = 'Scheduled'")
            .HasDatabaseName("IX_Visits_Scheduled_ExactDuplicate");
    }
}
