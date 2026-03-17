namespace ServiceDesk.Core.Entities;

/// <summary>
/// Групповой чат компании (только сотрудники, без клиентов)
/// </summary>
public class CompanyChat : BaseEntity
{
    /// <summary>Название чата</summary>
    public string Name { get; set; } = "Чат компании";

    /// <summary>Активен ли чат</summary>
    public bool IsActive { get; set; } = true;

    // Навигационные свойства
    public ICollection<CompanyChatMember> Members { get; set; } = new List<CompanyChatMember>();
    public ICollection<ChatMessage> Messages { get; set; } = new List<ChatMessage>();
}
