using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ServiceDesk.Core.Entities;

namespace ServiceDesk.Infrastructure.Data.Configurations;

public class ClientServicePointConfiguration : IEntityTypeConfiguration<ClientServicePoint>
{
    public void Configure(EntityTypeBuilder<ClientServicePoint> builder)
    {
        builder.ToTable("ClientServicePoints");

        // Уникальный индекс: одна связь клиент-точка не дублируется
        builder.HasIndex(csp => new { csp.ClientId, csp.ServicePointId }).IsUnique();

        builder.HasOne(csp => csp.Client)
            .WithMany(c => c.ClientServicePoints)
            .HasForeignKey(csp => csp.ClientId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(csp => csp.ServicePoint)
            .WithMany(sp => sp.ClientServicePoints)
            .HasForeignKey(csp => csp.ServicePointId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
