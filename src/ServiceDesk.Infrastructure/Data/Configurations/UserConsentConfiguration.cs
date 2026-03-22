using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ServiceDesk.Core.Entities;

namespace ServiceDesk.Infrastructure.Data.Configurations;

public class UserConsentConfiguration : IEntityTypeConfiguration<UserConsent>
{
    public void Configure(EntityTypeBuilder<UserConsent> builder)
    {
        builder.ToTable("UserConsents");

        builder.Property(c => c.ConsentVersion).HasMaxLength(20).IsRequired();
        builder.Property(c => c.ConsentText).HasColumnType("text").IsRequired();
        builder.Property(c => c.IpAddress).HasMaxLength(45);
        builder.Property(c => c.UserAgent).HasMaxLength(500);

        builder.HasIndex(c => new { c.UserId, c.ConsentType });

        builder.HasOne(c => c.User)
            .WithMany(u => u.Consents)
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
