using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ServiceDesk.Core.Entities;

namespace ServiceDesk.Infrastructure.Data.Configurations;

public class AppUserConfiguration : IEntityTypeConfiguration<AppUser>
{
    public void Configure(EntityTypeBuilder<AppUser> builder)
    {
        builder.ToTable("Users");

        builder.Property(u => u.LastName).HasMaxLength(100).IsRequired();
        builder.Property(u => u.FirstName).HasMaxLength(100).IsRequired();
        builder.Property(u => u.MiddleName).HasMaxLength(100);
        builder.Property(u => u.Phone).HasMaxLength(20).IsRequired();
        builder.Property(u => u.Email).HasMaxLength(255);
        builder.Property(u => u.Company).HasMaxLength(255);
        builder.Property(u => u.SmsCode).HasMaxLength(10);
        builder.Property(u => u.SecurityStamp).HasMaxLength(50).IsRequired();

        builder.HasIndex(u => u.Phone).IsUnique();

        builder.HasOne(u => u.Client)
            .WithMany(c => c.Managers)
            .HasForeignKey(u => u.ClientId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
