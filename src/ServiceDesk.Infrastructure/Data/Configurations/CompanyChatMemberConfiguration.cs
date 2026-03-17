using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ServiceDesk.Core.Entities;

namespace ServiceDesk.Infrastructure.Data.Configurations;

public class CompanyChatMemberConfiguration : IEntityTypeConfiguration<CompanyChatMember>
{
    public void Configure(EntityTypeBuilder<CompanyChatMember> builder)
    {
        builder.ToTable("CompanyChatMembers");

        // Уникальная пара: один пользователь — один раз в чате
        builder.HasIndex(m => new { m.CompanyChatId, m.UserId }).IsUnique();

        builder.HasOne(m => m.CompanyChat)
            .WithMany(c => c.Members)
            .HasForeignKey(m => m.CompanyChatId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(m => m.User)
            .WithMany()
            .HasForeignKey(m => m.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
