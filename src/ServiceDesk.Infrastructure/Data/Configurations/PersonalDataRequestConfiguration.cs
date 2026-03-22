using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ServiceDesk.Core.Entities;

namespace ServiceDesk.Infrastructure.Data.Configurations;

public class PersonalDataRequestConfiguration : IEntityTypeConfiguration<PersonalDataRequest>
{
    public void Configure(EntityTypeBuilder<PersonalDataRequest> builder)
    {
        builder.ToTable("PersonalDataRequests");

        builder.Property(r => r.Description).HasColumnType("text").IsRequired();
        builder.Property(r => r.Response).HasColumnType("text");

        builder.HasIndex(r => r.UserId);
        builder.HasIndex(r => r.Status);

        builder.HasOne(r => r.User)
            .WithMany(u => u.PersonalDataRequests)
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(r => r.ProcessedByUser)
            .WithMany()
            .HasForeignKey(r => r.ProcessedByUserId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
