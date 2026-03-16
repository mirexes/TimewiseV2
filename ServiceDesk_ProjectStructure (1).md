# Структура проекта ServiceDesk

## Решение (Solution)

```
ServiceDesk.sln
│
├── src/
│   ├── ServiceDesk.Web/                    ← ASP.NET Core MVC (точка входа)
│   ├── ServiceDesk.Application/            ← Бизнес-логика (сервисы)
│   ├── ServiceDesk.Core/                   ← Домен (сущности, интерфейсы, DTO)
│   └── ServiceDesk.Infrastructure/         ← Данные и внешние сервисы
│
└── tests/
    ├── ServiceDesk.Application.Tests/
    └── ServiceDesk.Web.Tests/
```

### Зависимости между проектами

```
Web → Application → Core
Web → Infrastructure → Core
```

Core ни от чего не зависит — это чистый домен.

---

## 1. ServiceDesk.Core (библиотека классов)

Чистый домен: сущности, перечисления, интерфейсы сервисов, DTO. Никаких зависимостей от EF, ASP.NET и т.д.

```
ServiceDesk.Core/
├── ServiceDesk.Core.csproj
│
├── Entities/
│   ├── Ticket.cs
│   ├── TicketSparePart.cs          ← связь «заявка — запчасть»
│   ├── Equipment.cs
│   ├── ServicePoint.cs
│   ├── Client.cs
│   ├── ContactPerson.cs
│   ├── SparePart.cs
│   ├── SparePartPriceHistory.cs
│   ├── SparePartStock.cs           ← остатки (склад + у инженеров)
│   ├── AppUser.cs
│   ├── ChatMessage.cs
│   ├── ChatAttachment.cs
│   ├── Notification.cs
│   ├── AuditLog.cs
│   └── BaseEntity.cs               ← Id, CreatedAt, UpdatedAt
│
├── Enums/
│   ├── TicketStatus.cs
│   ├── TicketType.cs
│   ├── TicketPriority.cs
│   ├── UserRole.cs
│   └── AuditAction.cs
│
├── Interfaces/
│   ├── Services/
│   │   ├── ITicketService.cs
│   │   ├── IEquipmentService.cs
│   │   ├── IClientService.cs
│   │   ├── ISparePartService.cs
│   │   ├── IChatService.cs
│   │   ├── INotificationService.cs
│   │   ├── IReportService.cs
│   │   ├── IAuditService.cs
│   │   ├── IAuthService.cs
│   │   ├── ISmsService.cs
│   │   └── IWebPushService.cs
│   │
│   └── Repositories/
│       └── IRepository.cs          ← generic: GetById, GetAll, Add, Update, Delete
│
└── DTOs/
    ├── Tickets/
    │   ├── TicketListDto.cs         ← для списка (лёгкий)
    │   ├── TicketDetailDto.cs       ← для карточки (полный)
    │   ├── CreateTicketDto.cs
    │   ├── UpdateTicketStatusDto.cs
    │   └── TicketFilterDto.cs       ← параметры фильтрации
    │
    ├── Equipment/
    │   ├── EquipmentDto.cs
    │   ├── EquipmentHistoryDto.cs
    │   └── CreateEquipmentDto.cs
    │
    ├── Chat/
    │   ├── ChatMessageDto.cs
    │   └── SendMessageDto.cs
    │
    ├── Reports/
    │   ├── DashboardStatsDto.cs
    │   ├── EngineerStatsDto.cs
    │   └── ReportFilterDto.cs
    │
    ├── Auth/
    │   ├── SendCodeDto.cs
    │   └── VerifyCodeDto.cs
    │
    └── Common/
        ├── PagedResultDto.cs        ← пагинация
        └── SelectOptionDto.cs       ← для выпадающих списков
```

### Пример: BaseEntity.cs

```csharp
namespace ServiceDesk.Core.Entities;

public abstract class BaseEntity
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
```

### Пример: TicketStatus.cs

```csharp
namespace ServiceDesk.Core.Enums;

public enum TicketStatus
{
    New = 0,                    // Новая
    Processed = 1,              // Обработана
    CompletedRemotely = 2,      // Выполнена дистанционно
    PartsApproval = 3,          // Согласование запчастей
    RepairApproved = 4,         // Ремонт согласован
    DepartureConfirmation = 5,  // Подтверждение выезда
    EngineerEnRoute = 6,        // Техник в пути
    InProgress = 7,             // Выполнение
    Completed = 8,              // Выполнена полностью
    ContinuationRequired = 9,   // Требуется продолжение работ
    Cancelled = 10              // Отменена
}
```

### Пример: ITicketService.cs

```csharp
namespace ServiceDesk.Core.Interfaces.Services;

public interface ITicketService
{
    Task<PagedResultDto<TicketListDto>> GetTicketsAsync(TicketFilterDto filter, int page, int pageSize);
    Task<TicketDetailDto?> GetByIdAsync(int id);
    Task<int> CreateAsync(CreateTicketDto dto, int currentUserId);
    Task UpdateStatusAsync(UpdateTicketStatusDto dto, int currentUserId);
    Task AssignEngineerAsync(int ticketId, int engineerId, int currentUserId);
    Task<IEnumerable<TicketListDto>> GetByEngineerAsync(int engineerId);
    Task AddSparePartsAsync(int ticketId, IEnumerable<TicketSparePartDto> parts, int currentUserId);
}
```

---

## 2. ServiceDesk.Infrastructure (библиотека классов)

Реализация доступа к данным (EF Core, MySQL) и внешних сервисов (SMS, Push).

```
ServiceDesk.Infrastructure/
├── ServiceDesk.Infrastructure.csproj
│
├── Data/
│   ├── AppDbContext.cs                      ← единый контекст
│   ├── Configurations/                      ← Fluent API
│   │   ├── TicketConfiguration.cs
│   │   ├── EquipmentConfiguration.cs
│   │   ├── ServicePointConfiguration.cs
│   │   ├── ClientConfiguration.cs
│   │   ├── SparePartConfiguration.cs
│   │   ├── AppUserConfiguration.cs
│   │   ├── ChatMessageConfiguration.cs
│   │   └── AuditLogConfiguration.cs
│   │
│   ├── Repositories/
│   │   └── Repository.cs                   ← generic реализация IRepository<T>
│   │
│   ├── Migrations/                          ← EF Core миграции
│   └── Seeds/
│       └── DbSeeder.cs                     ← начальные данные (роли, админ)
│
├── ExternalServices/
│   ├── SmsRuService.cs                     ← реализация ISmsService (SMS.ru)
│   └── WebPushService.cs                   ← реализация IWebPushService
│
└── DependencyInjection.cs                  ← extension method для регистрации
```

### Пример: AppDbContext.cs

```csharp
namespace ServiceDesk.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Ticket> Tickets => Set<Ticket>();
    public DbSet<Equipment> Equipment => Set<Equipment>();
    public DbSet<ServicePoint> ServicePoints => Set<ServicePoint>();
    public DbSet<Client> Clients => Set<Client>();
    public DbSet<SparePart> SpareParts => Set<SparePart>();
    public DbSet<AppUser> Users => Set<AppUser>();
    public DbSet<ChatMessage> ChatMessages => Set<ChatMessage>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<ContactPerson> ContactPersons => Set<ContactPerson>();
    public DbSet<SparePartStock> SparePartStocks => Set<SparePartStock>();
    public DbSet<SparePartPriceHistory> SparePartPriceHistory => Set<SparePartPriceHistory>();
    public DbSet<TicketSparePart> TicketSpareParts => Set<TicketSparePart>();
    public DbSet<ChatAttachment> ChatAttachments => Set<ChatAttachment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            if (entry.State == EntityState.Modified)
                entry.Entity.UpdatedAt = DateTime.UtcNow;
        }
        return base.SaveChangesAsync(cancellationToken);
    }
}
```

### Пример: DependencyInjection.cs

```csharp
namespace ServiceDesk.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseMySql(
                configuration.GetConnectionString("DefaultConnection"),
                ServerVersion.AutoDetect(configuration.GetConnectionString("DefaultConnection"))
            ));

        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<ISmsService, SmsRuService>();
        services.AddScoped<IWebPushService, WebPushService>();

        return services;
    }
}
```

---

## 3. ServiceDesk.Application (библиотека классов)

Вся бизнес-логика. Сервисы реализуют интерфейсы из Core.

```
ServiceDesk.Application/
├── ServiceDesk.Application.csproj
│
├── Services/
│   ├── TicketService.cs
│   ├── EquipmentService.cs
│   ├── ClientService.cs
│   ├── SparePartService.cs
│   ├── ChatService.cs
│   ├── NotificationService.cs
│   ├── ReportService.cs
│   ├── AuditService.cs
│   └── AuthService.cs
│
├── Mapping/
│   └── MappingExtensions.cs         ← ручной маппинг (без AutoMapper)
│
├── Validators/
│   ├── CreateTicketValidator.cs
│   ├── UpdateStatusValidator.cs
│   └── CreateEquipmentValidator.cs
│
├── Helpers/
│   ├── TicketStatusTransitions.cs   ← правила переходов статусов
│   └── PermissionChecker.cs         ← проверка прав по роли
│
└── DependencyInjection.cs           ← extension method для регистрации
```

### Пример: TicketService.cs (фрагмент)

```csharp
namespace ServiceDesk.Application.Services;

public class TicketService : ITicketService
{
    private readonly AppDbContext _db;
    private readonly IAuditService _audit;
    private readonly INotificationService _notifications;

    public TicketService(
        AppDbContext db,
        IAuditService audit,
        INotificationService notifications)
    {
        _db = db;
        _audit = audit;
        _notifications = notifications;
    }

    public async Task<PagedResultDto<TicketListDto>> GetTicketsAsync(
        TicketFilterDto filter, int page, int pageSize)
    {
        var query = _db.Tickets
            .Include(t => t.ServicePoint)
            .Include(t => t.AssignedEngineer)
            .AsQueryable();

        // Фильтрация
        if (filter.Status.HasValue)
            query = query.Where(t => t.Status == filter.Status.Value);

        if (filter.EngineerId.HasValue)
            query = query.Where(t => t.AssignedEngineerId == filter.EngineerId.Value);

        if (!string.IsNullOrEmpty(filter.Region))
            query = query.Where(t => t.ServicePoint.Region == filter.Region);

        if (!string.IsNullOrEmpty(filter.Network))
            query = query.Where(t => t.ServicePoint.Network == filter.Network);

        if (!string.IsNullOrEmpty(filter.SearchQuery))
        {
            var sq = filter.SearchQuery.ToLower();
            query = query.Where(t =>
                t.TicketNumber.ToLower().Contains(sq) ||
                t.Description.ToLower().Contains(sq) ||
                t.ServicePoint.Address.ToLower().Contains(sq));
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(t => t.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(t => t.ToListDto())  // маппинг
            .ToListAsync();

        return new PagedResultDto<TicketListDto>(items, totalCount, page, pageSize);
    }

    public async Task UpdateStatusAsync(UpdateTicketStatusDto dto, int currentUserId)
    {
        var ticket = await _db.Tickets.FindAsync(dto.TicketId)
            ?? throw new KeyNotFoundException($"Заявка {dto.TicketId} не найдена");

        // Проверка допустимости перехода
        if (!TicketStatusTransitions.IsAllowed(ticket.Status, dto.NewStatus))
            throw new InvalidOperationException(
                $"Переход {ticket.Status} → {dto.NewStatus} недопустим");

        var oldStatus = ticket.Status;
        ticket.Status = dto.NewStatus;
        ticket.Comment = dto.Comment;

        if (dto.NewStatus == TicketStatus.InProgress)
            ticket.WorkStartedAt = DateTime.UtcNow;

        if (dto.NewStatus == TicketStatus.Completed ||
            dto.NewStatus == TicketStatus.CompletedRemotely)
            ticket.WorkCompletedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        await _audit.LogAsync(AuditAction.StatusChanged, "Ticket", ticket.Id,
            oldStatus.ToString(), dto.NewStatus.ToString(), currentUserId);

        await _notifications.OnTicketStatusChangedAsync(ticket, oldStatus);
    }
}
```

### Пример: TicketStatusTransitions.cs

```csharp
namespace ServiceDesk.Application.Helpers;

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

    public static bool IsAllowed(TicketStatus from, TicketStatus to)
        => _transitions.TryGetValue(from, out var allowed) && allowed.Contains(to);

    public static TicketStatus[] GetAllowedTransitions(TicketStatus current)
        => _transitions.TryGetValue(current, out var allowed) ? allowed : [];
}
```

### Пример: DependencyInjection.cs

```csharp
namespace ServiceDesk.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<ITicketService, TicketService>();
        services.AddScoped<IEquipmentService, EquipmentService>();
        services.AddScoped<IClientService, ClientService>();
        services.AddScoped<ISparePartService, SparePartService>();
        services.AddScoped<IChatService, ChatService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<IReportService, ReportService>();
        services.AddScoped<IAuditService, AuditService>();
        services.AddScoped<IAuthService, AuthService>();

        return services;
    }
}
```

---

## 4. ServiceDesk.Web (ASP.NET Core MVC)

Точка входа. Тонкие контроллеры, Razor Views, статика, PWA.

```
ServiceDesk.Web/
├── ServiceDesk.Web.csproj
├── Program.cs
├── appsettings.json
├── appsettings.Development.json
│
├── Controllers/
│   ├── HomeController.cs               ← Dashboard
│   ├── AccountController.cs            ← Авторизация (SMS)
│   ├── TicketsController.cs            ← CRUD заявок
│   ├── EquipmentController.cs          ← Оборудование
│   ├── ClientsController.cs            ← Клиенты и точки
│   ├── SparePartsController.cs         ← Справочник запчастей
│   ├── ReportsController.cs            ← Отчёты и статистика
│   ├── UsersController.cs              ← Управление пользователями (модератор)
│   │
│   └── Api/                            ← AJAX-эндпоинты (JSON)
│       ├── ChatApiController.cs
│       ├── TicketsApiController.cs     ← смена статуса, фильтрация AJAX
│       └── NotificationsApiController.cs
│
├── Views/
│   ├── _ViewImports.cshtml
│   ├── _ViewStart.cshtml
│   │
│   ├── Shared/
│   │   ├── _Layout.cshtml              ← основной layout с темой
│   │   ├── _Sidebar.cshtml             ← навигация
│   │   ├── _ThemeSwitcher.cshtml       ← переключатель тем
│   │   ├── _Notifications.cshtml       ← колокольчик уведомлений
│   │   ├── _Pagination.cshtml          ← пагинация
│   │   ├── _StatusBadge.cshtml         ← цветной бейдж статуса
│   │   ├── Error.cshtml
│   │   └── Components/
│   │       ├── TicketCard/
│   │       │   └── Default.cshtml      ← Bootstrap card заявки
│   │       ├── FilterPanel/
│   │       │   └── Default.cshtml      ← панель фильтров
│   │       └── ChatWidget/
│   │           └── Default.cshtml      ← встроенный чат
│   │
│   ├── Home/
│   │   └── Dashboard.cshtml
│   │
│   ├── Account/
│   │   ├── Login.cshtml
│   │   └── VerifyCode.cshtml
│   │
│   ├── Tickets/
│   │   ├── Index.cshtml                ← список заявок
│   │   ├── Details.cshtml              ← карточка заявки + чат
│   │   ├── Create.cshtml
│   │   └── _TicketListPartial.cshtml   ← partial для AJAX-обновления
│   │
│   ├── Equipment/
│   │   ├── Index.cshtml
│   │   ├── Details.cshtml              ← карточка + история ремонта
│   │   └── Create.cshtml
│   │
│   ├── Clients/
│   │   ├── Index.cshtml
│   │   └── Details.cshtml
│   │
│   ├── SpareParts/
│   │   ├── Index.cshtml
│   │   └── Details.cshtml
│   │
│   ├── Reports/
│   │   ├── Dashboard.cshtml            ← графики (Chart.js)
│   │   ├── Engineers.cshtml
│   │   └── Export.cshtml
│   │
│   └── Users/
│       ├── Index.cshtml
│       ├── Create.cshtml
│       └── Edit.cshtml
│
├── ViewComponents/
│   ├── TicketCardViewComponent.cs
│   ├── FilterPanelViewComponent.cs
│   └── ChatWidgetViewComponent.cs
│
├── Filters/
│   ├── RoleAuthorizeAttribute.cs       ← [RoleAuthorize(UserRole.Moderator)]
│   └── AuditActionFilter.cs
│
├── Middleware/
│   └── ErrorHandlingMiddleware.cs
│
├── Extensions/
│   └── ClaimsPrincipalExtensions.cs    ← GetUserId(), GetRole()
│
└── wwwroot/
    ├── manifest.json                   ← PWA манифест
    ├── sw.js                           ← Service Worker
    │
    ├── css/
    │   ├── site.css                    ← общие стили
    │   └── themes/
    │       ├── _variables.css          ← CSS-переменные (единый файл)
    │       └── theme-overrides.css     ← переопределения для тёмной темы
    │
    ├── js/
    │   ├── site.js                     ← общая логика
    │   ├── theme-switcher.js           ← переключение тем
    │   ├── tickets.js                  ← AJAX для заявок
    │   ├── chat.js                     ← логика чата
    │   ├── notifications.js            ← Web Push подписка
    │   ├── offline-sync.js             ← синхронизация офлайн-изменений
    │   └── charts.js                   ← Chart.js инициализация
    │
    ├── icons/                          ← PWA иконки
    │   ├── icon-192.png
    │   └── icon-512.png
    │
    └── uploads/                        ← загрузки (фото АВР, чат-файлы)
        ├── avr/
        └── chat/
```

### Пример: TicketsController.cs (тонкий контроллер)

```csharp
namespace ServiceDesk.Web.Controllers;

[RoleAuthorize] // доступ для всех авторизованных
public class TicketsController : Controller
{
    private readonly ITicketService _ticketService;

    public TicketsController(ITicketService ticketService)
    {
        _ticketService = ticketService;
    }

    // GET /Tickets
    [HttpGet]
    public async Task<IActionResult> Index([FromQuery] TicketFilterDto filter, int page = 1)
    {
        var result = await _ticketService.GetTicketsAsync(filter, page, pageSize: 20);
        ViewBag.Filter = filter;
        return View(result);
    }

    // GET /Tickets/Details/5
    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var ticket = await _ticketService.GetByIdAsync(id);
        if (ticket is null) return NotFound();
        return View(ticket);
    }

    // GET /Tickets/Create
    [HttpGet]
    [RoleAuthorize(UserRole.Engineer, UserRole.Logist,
                   UserRole.ManagerTimewise, UserRole.ManagerClient,
                   UserRole.Moderator)]
    public IActionResult Create()
    {
        return View();
    }

    // POST /Tickets/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    [RoleAuthorize(UserRole.Engineer, UserRole.Logist,
                   UserRole.ManagerTimewise, UserRole.ManagerClient,
                   UserRole.Moderator)]
    public async Task<IActionResult> Create(CreateTicketDto dto)
    {
        if (!ModelState.IsValid) return View(dto);

        var id = await _ticketService.CreateAsync(dto, User.GetUserId());
        return RedirectToAction(nameof(Details), new { id });
    }
}
```

### Пример: Api/TicketsApiController.cs (AJAX)

```csharp
namespace ServiceDesk.Web.Controllers.Api;

[ApiController]
[Route("api/tickets")]
public class TicketsApiController : ControllerBase
{
    private readonly ITicketService _ticketService;

    public TicketsApiController(ITicketService ticketService)
    {
        _ticketService = ticketService;
    }

    // POST /api/tickets/status
    [HttpPost("status")]
    public async Task<IActionResult> UpdateStatus([FromBody] UpdateTicketStatusDto dto)
    {
        await _ticketService.UpdateStatusAsync(dto, User.GetUserId());
        return Ok(new { success = true });
    }

    // GET /api/tickets/filter
    [HttpGet("filter")]
    public async Task<IActionResult> Filter([FromQuery] TicketFilterDto filter, int page = 1)
    {
        var result = await _ticketService.GetTicketsAsync(filter, page, 20);
        return Ok(result);
    }
}
```

> **Правило:** MVC-контроллеры (наследуют `Controller`) используют `[HttpGet]` / `[HttpPost]` **без строки маршрута** — маршрут формируется из имени контроллера и действия. API-контроллеры (наследуют `ControllerBase`, аннотированы `[ApiController]`) используют `[Route("api/...")]` на классе и `[HttpPost("action")]` на методах.

---

## 5. Система тем (светлая + тёмная)

### _variables.css — CSS-переменные

```css
/* Светлая тема (по умолчанию) */
:root {
    --bg-primary: #ffffff;
    --bg-secondary: #f8f9fa;
    --bg-card: #ffffff;
    --bg-sidebar: #f1f3f5;

    --text-primary: #212529;
    --text-secondary: #6c757d;
    --text-muted: #adb5bd;

    --border-color: #dee2e6;
    --border-light: #e9ecef;

    --accent: #0d6efd;
    --accent-hover: #0b5ed7;
    --accent-light: #e7f1ff;

    --status-new: #0dcaf0;
    --status-processing: #ffc107;
    --status-done: #198754;
    --status-cancelled: #dc3545;
    --status-waiting: #fd7e14;

    --shadow-sm: 0 1px 2px rgba(0,0,0,0.05);
    --shadow-md: 0 4px 6px rgba(0,0,0,0.07);

    --radius-sm: 6px;
    --radius-md: 10px;
    --radius-lg: 14px;
}

/* Тёмная тема */
[data-theme="dark"] {
    --bg-primary: #1a1d21;
    --bg-secondary: #212529;
    --bg-card: #2b3035;
    --bg-sidebar: #16191d;

    --text-primary: #e9ecef;
    --text-secondary: #adb5bd;
    --text-muted: #6c757d;

    --border-color: #495057;
    --border-light: #343a40;

    --accent: #3d8bfd;
    --accent-hover: #6ea8fe;
    --accent-light: #1a2332;

    --shadow-sm: 0 1px 2px rgba(0,0,0,0.3);
    --shadow-md: 0 4px 6px rgba(0,0,0,0.4);
}
```

### theme-switcher.js

```javascript
(function () {
    const STORAGE_KEY = 'servicedesk-theme';

    function getPreferredTheme() {
        const stored = localStorage.getItem(STORAGE_KEY);
        if (stored) return stored;
        return window.matchMedia('(prefers-color-scheme: dark)').matches
            ? 'dark' : 'light';
    }

    function applyTheme(theme) {
        document.documentElement.setAttribute('data-theme', theme);
        localStorage.setItem(STORAGE_KEY, theme);

        // Обновляем Bootstrap theme
        document.documentElement.setAttribute(
            'data-bs-theme', theme
        );
    }

    // Применяем при загрузке
    applyTheme(getPreferredTheme());

    // Глобальная функция для кнопки
    window.toggleTheme = function () {
        const current = document.documentElement.getAttribute('data-theme');
        applyTheme(current === 'dark' ? 'light' : 'dark');
    };
})();
```

### _ThemeSwitcher.cshtml

```html
<button class="btn btn-outline-secondary btn-sm" onclick="toggleTheme()" 
        title="Сменить тему">
    <i class="bi bi-moon-fill d-none" id="icon-dark"></i>
    <i class="bi bi-sun-fill" id="icon-light"></i>
</button>
```

### Применение в стилях (site.css)

```css
body {
    background-color: var(--bg-secondary);
    color: var(--text-primary);
}

.card {
    background: var(--bg-card);
    border: 1px solid var(--border-light);
    border-radius: var(--radius-md);
    box-shadow: var(--shadow-sm);
}

.sidebar {
    background: var(--bg-sidebar);
    border-right: 1px solid var(--border-color);
}

/* Бейджи статусов */
.badge-status-new       { background: var(--status-new); }
.badge-status-done      { background: var(--status-done); }
.badge-status-cancelled { background: var(--status-cancelled); }
```

---

## 6. Program.cs

```csharp
using ServiceDesk.Application;
using ServiceDesk.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Регистрация модулей
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplication();

builder.Services.AddControllersWithViews();

builder.Services.AddAuthentication("Cookies")
    .AddCookie("Cookies", options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
    });

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Dashboard}/{id?}");

app.Run();
```

---

## 7. Карточка заявки (Bootstrap card, без table)

### TicketCard ViewComponent (Default.cshtml)

```html
@model TicketListDto

<div class="card mb-3 ticket-card" onclick="location.href='/Tickets/Details/@Model.Id'">
    <div class="card-body p-3">
        <div class="d-flex justify-content-between align-items-start mb-2">
            <div>
                <span class="fw-bold">@Model.TicketNumber</span>
                <span class="badge badge-status-@Model.Status.ToString().ToLower() ms-2">
                    @Model.StatusDisplayName
                </span>
            </div>
            <small class="text-muted">@Model.CreatedAt.ToString("dd.MM.yy HH:mm")</small>
        </div>

        <p class="mb-1 text-truncate">@Model.Description</p>

        <div class="d-flex justify-content-between align-items-center">
            <small class="text-muted">
                <i class="bi bi-geo-alt"></i> @Model.Address
            </small>
            <small class="text-muted">
                <i class="bi bi-person"></i> @Model.EngineerName
            </small>
        </div>

        @if (Model.Deadline.HasValue)
        {
            var isOverdue = Model.Deadline < DateTime.Now;
            <div class="mt-1">
                <small class="@(isOverdue ? "text-danger" : "text-muted")">
                    <i class="bi bi-clock"></i>
                    Дедлайн: @Model.Deadline.Value.ToString("dd.MM.yy HH:mm")
                </small>
            </div>
        }
    </div>
</div>
```

---

## 8. PWA: manifest.json

```json
{
    "name": "ServiceDesk — Управление заявками",
    "short_name": "ServiceDesk",
    "start_url": "/",
    "display": "standalone",
    "background_color": "#ffffff",
    "theme_color": "#0d6efd",
    "icons": [
        { "src": "/icons/icon-192.png", "sizes": "192x192", "type": "image/png" },
        { "src": "/icons/icon-512.png", "sizes": "512x512", "type": "image/png" }
    ]
}
```

---

## Сводка ключевых решений

| Решение | Обоснование |
|---------|-------------|
| 4 проекта (Core, Infrastructure, Application, Web) | Чёткое разделение ответственности, Core без зависимостей |
| Единый AppDbContext | Все сущности в одном контексте — простые запросы с JOIN |
| Тонкие контроллеры | Контроллер ≤ 5 строк на метод: принял → вызвал сервис → вернул |
| DI через extension methods | AddApplication() и AddInfrastructure() в Program.cs |
| [HttpGet] без строки маршрута | Маршрут = Controller/Action/Id (конвенция MVC) |
| CSS-переменные для тем | data-theme="dark" на html → все цвета меняются разом |
| Bootstrap cards вместо table | Адаптивность на мобильных из коробки |
| ViewComponents | Переиспользуемые UI-блоки (карточка заявки, фильтры, чат) |
| TicketStatusTransitions | Правила переходов в одном месте, легко тестировать |
| подробное комментирование кода на русском языке |
| красивый, удобный и лаконичный дизайн |
