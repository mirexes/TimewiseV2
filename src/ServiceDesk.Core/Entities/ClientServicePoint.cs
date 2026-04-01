namespace ServiceDesk.Core.Entities;

/// <summary>
/// Связь клиента с точкой обслуживания (многие-ко-многим).
/// Один адрес может принадлежать нескольким клиентам (например, ТЦ с несколькими арендаторами).
/// </summary>
public class ClientServicePoint : BaseEntity
{
    /// <summary>Клиент</summary>
    public int ClientId { get; set; }
    public Client Client { get; set; } = null!;

    /// <summary>Точка обслуживания</summary>
    public int ServicePointId { get; set; }
    public ServicePoint ServicePoint { get; set; } = null!;
}
