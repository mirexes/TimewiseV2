using ServiceDesk.Core.Enums;

namespace ServiceDesk.Application.Helpers;

/// <summary>
/// Проверка прав доступа по роли пользователя
/// </summary>
public static class PermissionChecker
{
    /// <summary>Может ли пользователь создавать заявки</summary>
    public static bool CanCreateTicket(UserRole role)
        => role is UserRole.Engineer or UserRole.ChiefEngineer or UserRole.Logist
            or UserRole.ManagerTimewise or UserRole.ManagerClient or UserRole.Moderator;

    /// <summary>Может ли пользователь назначать специалиста</summary>
    public static bool CanAssignEngineer(UserRole role)
        => role is UserRole.ChiefEngineer or UserRole.Logist or UserRole.Moderator;

    /// <summary>Может ли пользователь видеть все заявки</summary>
    public static bool CanViewAllTickets(UserRole role)
        => role is UserRole.ChiefEngineer or UserRole.Logist
            or UserRole.ManagerTimewise or UserRole.Moderator;

    /// <summary>Может ли пользователь отменять заявки</summary>
    public static bool CanCancelTicket(UserRole role)
        => role is UserRole.ManagerTimewise or UserRole.Moderator;

    /// <summary>Может ли пользователь управлять пользователями</summary>
    public static bool CanManageUsers(UserRole role)
        => role == UserRole.Moderator;

    /// <summary>Может ли пользователь согласовывать запчасти</summary>
    public static bool CanApproveParts(UserRole role)
        => role is UserRole.ChiefEngineer or UserRole.ManagerTimewise
            or UserRole.ManagerClient or UserRole.Moderator;

    /// <summary>Может ли пользователь просматривать отчёты</summary>
    public static bool CanViewReports(UserRole role)
        => role is UserRole.ChiefEngineer or UserRole.ManagerTimewise or UserRole.Moderator;

    /// <summary>Может ли пользователь управлять клиентами (просмотр, создание, редактирование)</summary>
    public static bool CanManageClients(UserRole role)
        => role is UserRole.ChiefEngineer or UserRole.Logist
            or UserRole.ManagerTimewise or UserRole.Moderator;
}
