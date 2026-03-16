namespace ServiceDesk.Core.Entities;

/// <summary>
/// Запасная часть
/// </summary>
public class SparePart : BaseEntity
{
    /// <summary>Артикул</summary>
    public string Article { get; set; } = string.Empty;

    /// <summary>Название</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Описание</summary>
    public string? Description { get; set; }

    /// <summary>Текущая цена</summary>
    public decimal Price { get; set; }

    /// <summary>Признак дорогостоящей запчасти (требует согласования)</summary>
    public bool IsExpensive { get; set; }

    /// <summary>Активна ли запчасть в справочнике</summary>
    public bool IsActive { get; set; } = true;

    // Навигационные свойства
    public ICollection<SparePartPriceHistory> PriceHistory { get; set; } = new List<SparePartPriceHistory>();
    public ICollection<SparePartStock> Stocks { get; set; } = new List<SparePartStock>();
    public ICollection<TicketSparePart> TicketSpareParts { get; set; } = new List<TicketSparePart>();
}
