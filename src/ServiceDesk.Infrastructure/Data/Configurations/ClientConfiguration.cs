using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ServiceDesk.Core.Entities;

namespace ServiceDesk.Infrastructure.Data.Configurations;

public class ClientConfiguration : IEntityTypeConfiguration<Client>
{
    public void Configure(EntityTypeBuilder<Client> builder)
    {
        builder.ToTable("Clients");

        builder.Property(c => c.Name).HasMaxLength(300).IsRequired();
        builder.Property(c => c.Inn).HasMaxLength(12);
        builder.Property(c => c.Network).HasMaxLength(200);
        builder.Property(c => c.Phone).HasMaxLength(20);
        builder.Property(c => c.Email).HasMaxLength(255);
        builder.Property(c => c.LegalAddress).HasMaxLength(500);
        builder.Property(c => c.TtkFilePath).HasMaxLength(500);
    }
}
