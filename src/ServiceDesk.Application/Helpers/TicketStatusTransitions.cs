using ServiceDesk.Core.Enums;

namespace ServiceDesk.Application.Helpers;

/// <summary>
/// Правила допустимых переходов статусов заявки (единое место)
/// </summary>
public static class TicketStatusTransitions
{
    private static readonly Dictionary<TicketStatus, TicketStatus[]> _transitions = new()
    {
        [TicketStatus.New] = [TicketStatus.Processed, TicketStatus.Cancelled],
        [TicketStatus.Processed] = [TicketStatus.CompletedRemotely,
            TicketStatus.PartsApproval, TicketStatus.DepartureConfirmation],
        [TicketStatus.PartsApproval] = [TicketStatus.RepairApproved, TicketStatus.Cancelled],
        [TicketStatus.RepairApproved] = [TicketStatus.DepartureConfirmation],
        [TicketStatus.DepartureConfirmation] = [TicketStatus.EngineerEnRoute],
        [TicketStatus.EngineerEnRoute] = [TicketStatus.InProgress],
        [TicketStatus.InProgress] = [TicketStatus.Completed,
            TicketStatus.ContinuationRequired, TicketStatus.PartsApproval],
        [TicketStatus.ContinuationRequired] = [TicketStatus.DepartureConfirmation,
            TicketStatus.PartsApproval],
    };

    /// <summary>Проверяет, допустим ли переход из одного статуса в другой</summary>
    public static bool IsAllowed(TicketStatus from, TicketStatus to)
        => _transitions.TryGetValue(from, out var allowed) && allowed.Contains(to);

    /// <summary>Возвращает массив допустимых переходов из текущего статуса</summary>
    public static TicketStatus[] GetAllowedTransitions(TicketStatus current)
        => _transitions.TryGetValue(current, out var allowed) ? allowed : [];
}
