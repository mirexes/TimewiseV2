using Microsoft.EntityFrameworkCore;
using ServiceDesk.Core.Entities;

namespace ServiceDesk.Infrastructure.Data;

/// <summary>
/// Единый контекст базы данных
/// </summary>
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Ticket> Tickets => Set<Ticket>();
    public DbSet<Equipment> Equipment => Set<Equipment>();
    public DbSet<ServicePoint> ServicePoints => Set<ServicePoint>();
    public DbSet<Client> Clients => Set<Client>();
    public DbSet<SparePart> SpareParts => Set<SparePart>();
    public DbSet<AppUser> Users => Set<AppUser>();
    public DbSet<ChatMessage> ChatMessages => Set<ChatMessage>();
    public DbSet<ChatAttachment> ChatAttachments => Set<ChatAttachment>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<ContactPerson> ContactPersons => Set<ContactPerson>();
    public DbSet<SparePartStock> SparePartStocks => Set<SparePartStock>();
    public DbSet<SparePartPriceHistory> SparePartPriceHistory => Set<SparePartPriceHistory>();
    public DbSet<TicketSparePart> TicketSpareParts => Set<TicketSparePart>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Применяем все конфигурации из текущей сборки
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Автоматическое обновление UpdatedAt при изменении сущности
        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            if (entry.State == EntityState.Modified)
                entry.Entity.UpdatedAt = DateTime.UtcNow;
        }
        return base.SaveChangesAsync(cancellationToken);
    }
}
