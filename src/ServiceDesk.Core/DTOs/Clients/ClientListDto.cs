namespace ServiceDesk.Core.DTOs.Clients;

/// <summary>
/// DTO клиента для списка
/// </summary>
public class ClientListDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Inn { get; set; }
    public string? Network { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public bool IsActive { get; set; }
    public int ServicePointsCount { get; set; }
    public int ContactPersonsCount { get; set; }
    public DateTime CreatedAt { get; set; }
}
