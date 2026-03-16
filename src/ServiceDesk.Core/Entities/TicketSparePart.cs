namespace ServiceDesk.Core.Entities;

/// <summary>
/// Связь «заявка — запчасть» (многие-ко-многим)
/// </summary>
public class TicketSparePart : BaseEntity
{
    public int TicketId { get; set; }
    public Ticket Ticket { get; set; } = null!;

    public int SparePartId { get; set; }
    public SparePart SparePart { get; set; } = null!;

    /// <summary>Количество</summary>
    public int Quantity { get; set; } = 1;

    /// <summary>Цена на момент добавления</summary>
    public decimal PriceAtTime { get; set; }

    /// <summary>Согласована ли запчасть</summary>
    public bool IsApproved { get; set; }
}
