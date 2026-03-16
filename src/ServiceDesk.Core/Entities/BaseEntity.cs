namespace ServiceDesk.Core.Entities;

/// <summary>
/// Базовая сущность с общими полями для всех таблиц
/// </summary>
public abstract class BaseEntity
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
