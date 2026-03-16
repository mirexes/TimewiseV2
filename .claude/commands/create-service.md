# Создание сервиса и контроллера для ServiceDesk

Создай сервис (бизнес-логика) и контроллер для указанной сущности.

## Входные данные
Пользователь указывает: название сущности или модуля. Если не указал — спроси.

## Шаг 1: Интерфейс сервиса
Файл: `ServiceDesk.Core/Interfaces/Services/I{Entity}Service.cs`

```csharp
namespace ServiceDesk.Core.Interfaces.Services;

public interface ITicketService
{
    Task<PagedResultDto<{Entity}ListDto>> GetAllAsync({Entity}FilterDto filter, int page, int pageSize);
    Task<{Entity}DetailDto?> GetByIdAsync(int id);
    Task<int> CreateAsync(Create{Entity}Dto dto, int currentUserId);
    Task UpdateAsync(int id, Update{Entity}Dto dto, int currentUserId);
    Task DeleteAsync(int id, int currentUserId);
}
```

## Шаг 2: Реализация сервиса
Файл: `ServiceDesk.Application/Services/{Entity}Service.cs`

Правила:
- Внедряй `AppDbContext`, `IAuditService`, `INotificationService` через конструктор
- Используй `async/await` везде
- Фильтрация через `IQueryable` с условными `.Where()`
- Пагинация: `Skip((page-1)*pageSize).Take(pageSize)`
- Сортировка по умолчанию: `OrderByDescending(x => x.CreatedAt)`
- Маппинг — через extension methods из `MappingExtensions.cs`
- Логирование изменений через `IAuditService.LogAsync()`
- Комментарии на русском языке
- Выбрасывай `KeyNotFoundException` если сущность не найдена
- Проверяй права доступа через `PermissionChecker` если нужно

## Шаг 3: MVC-контроллер
Файл: `ServiceDesk.Web/Controllers/{Entity}Controller.cs` (если нужны страницы)

Правила:
- Наследует `Controller`
- Атрибут `[RoleAuthorize]` на классе (или конкретные роли на методах)
- `[HttpGet]`/`[HttpPost]` **без строки маршрута**
- Контроллер **тонкий**: максимум 5 строк на метод
- `ValidateAntiForgeryToken` на POST
- Используй `User.GetUserId()` для текущего пользователя

## Шаг 4: API-контроллер (если нужен AJAX)
Файл: `ServiceDesk.Web/Controllers/Api/{Entity}ApiController.cs`

Правила:
- Наследует `ControllerBase`
- `[ApiController]` + `[Route("api/{entities}")]`
- Возвращает `Ok(new { success = true, data = ... })`

## Шаг 5: Регистрация DI
Добавь в `ServiceDesk.Application/DependencyInjection.cs`:
```csharp
services.AddScoped<I{Entity}Service, {Entity}Service>();
```

$ARGUMENTS
