namespace ServiceDesk.Core.Entities;

/// <summary>
/// Точка обслуживания
/// </summary>
public class ServicePoint : BaseEntity
{
    /// <summary>Название точки</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Адрес</summary>
    public string Address { get; set; } = string.Empty;

    /// <summary>Регион</summary>
    public string? Region { get; set; }

    /// <summary>Сеть (название сети клиента)</summary>
    public string? Network { get; set; }

    /// <summary>Широта</summary>
    public double? Latitude { get; set; }

    /// <summary>Долгота</summary>
    public double? Longitude { get; set; }

    /// <summary>Контактный телефон</summary>
    public string? ContactPhone { get; set; }

    /// <summary>Контактное лицо</summary>
    public string? ContactPerson { get; set; }

    /// <summary>Активна ли точка</summary>
    public bool IsActive { get; set; } = true;

    /// <summary>Клиент (организация)</summary>
    public int ClientId { get; set; }
    public Client Client { get; set; } = null!;

    // Навигационные свойства
    public ICollection<Equipment> Equipment { get; set; } = new List<Equipment>();
    public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
}
