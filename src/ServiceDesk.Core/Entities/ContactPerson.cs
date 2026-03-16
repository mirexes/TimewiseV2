namespace ServiceDesk.Core.Entities;

/// <summary>
/// Контактное лицо клиента
/// </summary>
public class ContactPerson : BaseEntity
{
    public string FullName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Position { get; set; }

    public int ClientId { get; set; }
    public Client Client { get; set; } = null!;
}
