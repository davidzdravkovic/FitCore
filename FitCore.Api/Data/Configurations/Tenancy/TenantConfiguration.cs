using FitCore.Api.Domain.Tenants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FitCore.Api.Data.Configurations.Tenancy;

public sealed class TenantConfiguration : IEntityTypeConfiguration<Tenant>
{
    public void Configure(EntityTypeBuilder<Tenant> entity)
    {
        entity.Property(t => t.Status).HasConversion<string>();
        entity.Property(t => t.Currency).HasMaxLength(3);
    }
}
