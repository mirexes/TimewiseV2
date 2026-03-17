namespace ServiceDesk.Core.DTOs.Tickets;

/// <summary>
/// DTO для выбора специалиста при назначении на заявку
/// </summary>
public class EngineerSelectDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}
