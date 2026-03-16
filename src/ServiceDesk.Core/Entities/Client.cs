namespace ServiceDesk.Core.Entities;

/// <summary>
/// Клиент (организация)
/// </summary>
public class Client : BaseEntity
{
    /// <summary>Наименование организации</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>ИНН</summary>
    public string? Inn { get; set; }

    /// <summary>Сеть</summary>
    public string? Network { get; set; }

    /// <summary>Контактный телефон</summary>
    public string? Phone { get; set; }

    /// <summary>Email</summary>
    public string? Email { get; set; }

    /// <summary>Адрес (юридический)</summary>
    public string? LegalAddress { get; set; }

    /// <summary>Активен ли клиент</summary>
    public bool IsActive { get; set; } = true;

    /// <summary>ТТК (технико-технологическая карта)</summary>
    public string? TtkFilePath { get; set; }

    // Навигационные свойства
    public ICollection<ServicePoint> ServicePoints { get; set; } = new List<ServicePoint>();
    public ICollection<ContactPerson> ContactPersons { get; set; } = new List<ContactPerson>();
    public ICollection<AppUser> Managers { get; set; } = new List<AppUser>();
}
