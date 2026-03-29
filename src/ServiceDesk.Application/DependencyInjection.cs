using Microsoft.Extensions.DependencyInjection;
using ServiceDesk.Application.Services;
using ServiceDesk.Core.Interfaces.Services;

namespace ServiceDesk.Application;

/// <summary>
/// Регистрация сервисов бизнес-логики
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<ITicketService, TicketService>();
        services.AddScoped<IEquipmentService, EquipmentService>();
        services.AddScoped<IClientService, ClientService>();
        services.AddScoped<IChatService, ChatService>();
        services.AddScoped<ICompanyChatService, CompanyChatService>();
        services.AddScoped<IDirectChatService, DirectChatService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<IReportService, ReportService>();
        services.AddScoped<IAuditService, AuditService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IPersonalDataService, PersonalDataService>();

        return services;
    }
}
