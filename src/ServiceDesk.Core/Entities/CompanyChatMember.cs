namespace ServiceDesk.Core.Entities;

/// <summary>
/// Участник группового чата компании (добавляет только модератор)
/// </summary>
public class CompanyChatMember : BaseEntity
{
    /// <summary>Групповой чат</summary>
    public int CompanyChatId { get; set; }
    public CompanyChat CompanyChat { get; set; } = null!;

    /// <summary>Пользователь-участник</summary>
    public int UserId { get; set; }
    public AppUser User { get; set; } = null!;
}
