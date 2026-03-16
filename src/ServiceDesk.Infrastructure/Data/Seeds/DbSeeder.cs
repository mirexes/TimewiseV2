using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ServiceDesk.Core.Entities;
using ServiceDesk.Core.Enums;

namespace ServiceDesk.Infrastructure.Data.Seeds;

/// <summary>
/// Начальное заполнение базы данных
/// </summary>
public static class DbSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        // Применяем миграции
        await db.Database.MigrateAsync();

        // Создаём модератора (администратора) если нет пользователей
        if (!await db.Users.AnyAsync())
        {
            var moderator = new AppUser
            {
                LastName = "Администратор",
                FirstName = "Системный",
                Phone = "+70000000000",
                Role = UserRole.Moderator,
                IsActive = true
            };
            db.Users.Add(moderator);

            // Тестовый клиент
            var client = new Client
            {
                Name = "ООО «Тестовая компания»",
                Inn = "1234567890",
                Network = "Тестовая сеть",
                Phone = "+71111111111",
                Email = "test@example.com"
            };
            db.Clients.Add(client);
            await db.SaveChangesAsync();

            // Тестовая точка обслуживания
            var servicePoint = new ServicePoint
            {
                Name = "Офис на Ленина",
                Address = "г. Москва, ул. Ленина, д. 1",
                Region = "Москва",
                Network = "Тестовая сеть",
                ClientId = client.Id,
                Latitude = 55.7558,
                Longitude = 37.6173
            };
            db.ServicePoints.Add(servicePoint);
            await db.SaveChangesAsync();

            // Тестовое оборудование
            var equipment = new Equipment
            {
                Model = "DeLonghi ECAM 370.95.T",
                SerialNumber = "SN-2024-001",
                InstalledAt = DateTime.UtcNow.AddMonths(-6),
                ServicePointId = servicePoint.Id,
                Description = "Кофемашина автоматическая"
            };
            db.Equipment.Add(equipment);
            await db.SaveChangesAsync();
        }
    }
}
