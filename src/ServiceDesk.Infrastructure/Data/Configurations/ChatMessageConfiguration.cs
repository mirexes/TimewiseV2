using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ServiceDesk.Core.Entities;

namespace ServiceDesk.Infrastructure.Data.Configurations;

public class ChatMessageConfiguration : IEntityTypeConfiguration<ChatMessage>
{
    public void Configure(EntityTypeBuilder<ChatMessage> builder)
    {
        builder.ToTable("ChatMessages");

        builder.Property(cm => cm.Text).HasMaxLength(4000).IsRequired();

        builder.HasOne(cm => cm.Ticket)
            .WithMany(t => t.ChatMessages)
            .HasForeignKey(cm => cm.TicketId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(cm => cm.Sender)
            .WithMany(u => u.ChatMessages)
            .HasForeignKey(cm => cm.SenderId)
            .OnDelete(DeleteBehavior.Restrict);

        // Ответ на сообщение (самоссылка)
        builder.HasOne(cm => cm.ReplyToMessage)
            .WithMany(cm => cm.Replies)
            .HasForeignKey(cm => cm.ReplyToMessageId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
