namespace ServiceDesk.Core.Entities;

/// <summary>
/// История изменения цены запчасти
/// </summary>
public class SparePartPriceHistory : BaseEntity
{
    public int SparePartId { get; set; }
    public SparePart SparePart { get; set; } = null!;

    public decimal OldPrice { get; set; }
    public decimal NewPrice { get; set; }
    public DateTime ChangedAt { get; set; } = DateTime.UtcNow;
}
