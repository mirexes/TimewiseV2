using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ServiceDesk.Core.Entities;

namespace ServiceDesk.Infrastructure.Data.Configurations;

public class TicketConfiguration : IEntityTypeConfiguration<Ticket>
{
    public void Configure(EntityTypeBuilder<Ticket> builder)
    {
        builder.ToTable("Tickets");

        builder.Property(t => t.TicketNumber).HasMaxLength(20).IsRequired();
        builder.Property(t => t.Description).HasMaxLength(2000).IsRequired();
        builder.Property(t => t.Comment).HasMaxLength(1000);
        builder.Property(t => t.AvrPhotoPath).HasMaxLength(500);

        builder.HasIndex(t => t.TicketNumber).IsUnique();
        builder.HasIndex(t => t.Status);
        builder.HasIndex(t => t.CreatedAt);

        builder.HasOne(t => t.AssignedEngineer)
            .WithMany(u => u.AssignedTickets)
            .HasForeignKey(t => t.AssignedEngineerId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(t => t.ServicePoint)
            .WithMany(sp => sp.Tickets)
            .HasForeignKey(t => t.ServicePointId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(t => t.Equipment)
            .WithMany(e => e.Tickets)
            .HasForeignKey(t => t.EquipmentId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(t => t.CreatedByUser)
            .WithMany()
            .HasForeignKey(t => t.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
