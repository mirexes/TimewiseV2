using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ServiceDesk.Core.Entities;

namespace ServiceDesk.Infrastructure.Data.Configurations;

public class EquipmentConfiguration : IEntityTypeConfiguration<Equipment>
{
    public void Configure(EntityTypeBuilder<Equipment> builder)
    {
        builder.ToTable("Equipment");

        builder.Property(e => e.Model).HasMaxLength(200).IsRequired();
        builder.Property(e => e.SerialNumber).HasMaxLength(100).IsRequired();
        builder.Property(e => e.Description).HasMaxLength(1000);

        builder.HasIndex(e => e.SerialNumber).IsUnique();

        builder.HasOne(e => e.ServicePoint)
            .WithMany(sp => sp.Equipment)
            .HasForeignKey(e => e.ServicePointId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
