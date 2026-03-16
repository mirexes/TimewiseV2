# Создание сущности (Entity) для ServiceDesk

Создай новую сущность в проекте ServiceDesk.Core/Entities/ по следующим правилам:

## Входные данные
Пользователь указывает: название сущности и её атрибуты (поля). Если не указал — определи из ТЗ (TZ_Sistema_v3.0.docx) и структуры проекта (ServiceDesk_ProjectStructure (1).md).

## Правила создания

1. **Наследование**: сущность наследует `BaseEntity` (Id, CreatedAt, UpdatedAt)
2. **Namespace**: `ServiceDesk.Core.Entities`
3. **Навигационные свойства**: добавляй связи (FK + навигацию) по смыслу сущности
4. **Именование**:
   - Класс — PascalCase, единственное число (Ticket, Equipment)
   - FK-свойство: `{Navigation}Id` (например, `ServicePointId`)
   - Коллекции: `ICollection<T>` с инициализацией `new List<T>()`
5. **Enum-поля**: используй enum из `ServiceDesk.Core.Enums/` (TicketStatus, TicketType, TicketPriority, UserRole, AuditAction)
6. **Комментарии**: XML-комментарии на русском языке для каждого свойства
7. **Nullable**: используй `?` для необязательных полей

## После создания сущности
1. Создай конфигурацию Fluent API в `ServiceDesk.Infrastructure/Data/Configurations/{Entity}Configuration.cs`
2. Добавь `DbSet<{Entity}>` в `AppDbContext.cs`
3. Создай DTO в `ServiceDesk.Core/DTOs/{EntityFolder}/`:
   - `{Entity}Dto.cs` — для чтения
   - `Create{Entity}Dto.cs` — для создания
4. Добавь маппинг в `ServiceDesk.Application/Mapping/MappingExtensions.cs`

## Пример сущности

```csharp
namespace ServiceDesk.Core.Entities;

/// <summary>
/// Оборудование, установленное у клиента
/// </summary>
public class Equipment : BaseEntity
{
    /// <summary>Название/модель оборудования</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Серийный номер (уникальный)</summary>
    public string SerialNumber { get; set; } = string.Empty;

    /// <summary>Дата установки</summary>
    public DateTime? InstalledAt { get; set; }

    /// <summary>Точка обслуживания</summary>
    public int ServicePointId { get; set; }
    public ServicePoint ServicePoint { get; set; } = null!;

    /// <summary>Заявки по данному оборудованию (история ремонта)</summary>
    public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
}
```

## Пример конфигурации

```csharp
namespace ServiceDesk.Infrastructure.Data.Configurations;

public class EquipmentConfiguration : IEntityTypeConfiguration<Equipment>
{
    public void Configure(EntityTypeBuilder<Equipment> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Name).IsRequired().HasMaxLength(200);
        builder.Property(e => e.SerialNumber).IsRequired().HasMaxLength(100);
        builder.HasIndex(e => e.SerialNumber).IsUnique();
        builder.HasOne(e => e.ServicePoint)
            .WithMany(sp => sp.Equipment)
            .HasForeignKey(e => e.ServicePointId);
    }
}
```

$ARGUMENTS
