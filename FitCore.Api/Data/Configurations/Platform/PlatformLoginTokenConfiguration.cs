using FitCore.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FitCore.Api.Data.Configurations.Platform;

public sealed class PlatformLoginTokenConfiguration : IEntityTypeConfiguration<PlatformLoginToken>
{
    public void Configure(EntityTypeBuilder<PlatformLoginToken> entity)
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
    }
}
