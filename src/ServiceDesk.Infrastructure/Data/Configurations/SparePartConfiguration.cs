using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ServiceDesk.Core.Entities;

namespace ServiceDesk.Infrastructure.Data.Configurations;

public class SparePartConfiguration : IEntityTypeConfiguration<SparePart>
{
    public void Configure(EntityTypeBuilder<SparePart> builder)
    {
        builder.ToTable("SpareParts");

        builder.Property(sp => sp.Article).HasMaxLength(50).IsRequired();
        builder.Property(sp => sp.Name).HasMaxLength(300).IsRequired();
        builder.Property(sp => sp.Description).HasMaxLength(1000);
        builder.Property(sp => sp.Price).HasPrecision(18, 2);

        builder.HasIndex(sp => sp.Article).IsUnique();
    }
}
