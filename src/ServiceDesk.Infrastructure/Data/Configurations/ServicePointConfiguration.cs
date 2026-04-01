using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ServiceDesk.Core.Entities;

namespace ServiceDesk.Infrastructure.Data.Configurations;

public class ServicePointConfiguration : IEntityTypeConfiguration<ServicePoint>
{
    public void Configure(EntityTypeBuilder<ServicePoint> builder)
    {
        builder.ToTable("ServicePoints");

        builder.Property(sp => sp.Name).HasMaxLength(200).IsRequired();
        builder.Property(sp => sp.Address).HasMaxLength(500).IsRequired();
        builder.Property(sp => sp.Region).HasMaxLength(100);
        builder.Property(sp => sp.Network).HasMaxLength(200);
        builder.Property(sp => sp.ContactPhone).HasMaxLength(20);
        builder.Property(sp => sp.ContactPerson).HasMaxLength(200);
    }
}
