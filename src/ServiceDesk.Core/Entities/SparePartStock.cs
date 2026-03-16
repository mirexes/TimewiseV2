namespace ServiceDesk.Core.Entities;

/// <summary>
/// Остатки запчастей (склад + у инженеров)
/// </summary>
public class SparePartStock : BaseEntity
{
    public int SparePartId { get; set; }
    public SparePart SparePart { get; set; } = null!;

    /// <summary>Количество</summary>
    public int Quantity { get; set; }

    /// <summary>Местоположение (склад или ID инженера)</summary>
    public string Location { get; set; } = string.Empty;

    /// <summary>У какого инженера (null = на складе)</summary>
    public int? EngineerId { get; set; }
    public AppUser? Engineer { get; set; }
}
