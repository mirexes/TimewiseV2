using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ServiceDesk.Core.Entities;

namespace ServiceDesk.Infrastructure.Data.Configurations;

public class TicketPhotoConfiguration : IEntityTypeConfiguration<TicketPhoto>
{
    public void Configure(EntityTypeBuilder<TicketPhoto> builder)
    {
        builder.ToTable("TicketPhotos");

        builder.Property(p => p.FileName).HasMaxLength(255).IsRequired();
        builder.Property(p => p.FilePath).HasMaxLength(500).IsRequired();
        builder.Property(p => p.ContentType).HasMaxLength(100).IsRequired();

        builder.HasOne(p => p.Ticket)
            .WithMany(t => t.CompletionPhotos)
            .HasForeignKey(p => p.TicketId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
