using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ServiceDesk.Core.Interfaces.Repositories;
using ServiceDesk.Core.Interfaces.Services;
using ServiceDesk.Infrastructure.Data;
using ServiceDesk.Infrastructure.Data.Repositories;
using ServiceDesk.Infrastructure.ExternalServices;

namespace ServiceDesk.Infrastructure;

/// <summary>
/// Регистрация сервисов инфраструктурного слоя
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services, IConfiguration configuration)
    {
        // MySQL через Pomelo
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        services.AddDbContext<AppDbContext>(options =>
            options.UseMySql(
                connectionString,
                ServerVersion.AutoDetect(connectionString)
            ));

        // Хранение ключей Data Protection в БД для сохранения авторизации между перезапусками
        services.AddDataProtection()
            .PersistKeysToDbContext<AppDbContext>()
            .SetApplicationName("ServiceDesk");

        // Обобщённый репозиторий
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

        // Внешние сервисы
        services.AddScoped<ISmsService, SmsRuService>();
        services.AddScoped<IWebPushService, WebPushService>();

        return services;
    }
}
