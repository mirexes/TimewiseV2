# ServiceDesk — Система управления заявками на обслуживание оборудования

## Обзор проекта
Веб-приложение (PWA) для управления заявками на ремонт, обслуживание и поставки кофемашин и сопутствующего оборудования. ASP.NET Core MVC + MySQL + EF Core.

## Архитектура (4 проекта)
```
ServiceDesk.sln
├── src/
│   ├── ServiceDesk.Core/            — Домен: сущности, enum, интерфейсы, DTO (без зависимостей)
│   ├── ServiceDesk.Infrastructure/  — EF Core, MySQL, внешние сервисы (SMS, WebPush)
│   ├── ServiceDesk.Application/     — Бизнес-логика, сервисы, валидаторы
│   └── ServiceDesk.Web/             — ASP.NET Core MVC, контроллеры, Razor Views, PWA
└── tests/
    ├── ServiceDesk.Application.Tests/
    └── ServiceDesk.Web.Tests/
```

### Зависимости
```
Web → Application → Core
Web → Infrastructure → Core
```
Core ни от чего не зависит.

## Технологический стек
- **Backend**: ASP.NET Core 9+ MVC, C#, Razor Views (.cshtml)
- **ORM**: Entity Framework Core 9+ (Code First)
- **БД**: MySQL 8.x
- **CSS**: Bootstrap 5.3, CSS-переменные для тем (светлая/тёмная)
- **JS**: Vanilla JavaScript (БЕЗ jQuery), Fetch API
- **Графики**: Chart.js 4.x
- **Уведомления**: Web Push API (VAPID)
- **SMS**: SMS.ru / SMSC.ru
- **Карты**: Yandex Maps JavaScript API
- **Иконки**: Bootstrap Icons / Font Awesome
- **Деплой**: Docker + Docker Compose, Kestrel + Nginx, Ubuntu 22/24 LTS
- **CI/CD**: GitHub Actions + Docker Registry
- **Логирование**: Serilog + Seq

## 8 ролей пользователей
1. **Техник** — выездной, видит только свои заявки, офлайн-доступ
2. **Инженер** — аналогично технику (разница должностная)
3. **Главный инженер** — видит все заявки, переназначает, согласует запчасти
4. **Логист** — создание, обработка, назначение, дистанционный ремонт
5. **Менеджер Timewise** — согласование, отчёты, отмена заявок
6. **Клиент** — только свои заявки (роль по умолчанию при регистрации)
7. **Менеджер клиента** — привязан к сети модератором, согласует запчасти
8. **Модератор** — полный доступ, управление пользователями, ФЗ-152

## 11 статусов заявки
```
New → Processed → CompletedRemotely
                → PartsApproval → RepairApproved → InProgress → EngineerEnRoute → Execution → Completed
                                                                                            → ContinuationRequired → InProgress (цикл)
                → InProgress (без запчастей)
Любой → Cancelled (Менеджер TW / Модератор)
```

## Ключевые сущности
- **Ticket** — заявка (номер, тип, статус, приоритет, дедлайн, специалист, оборудование, точка, АВР, запчасти, чат)
- **Equipment** — оборудование (модель, серийный номер, дата установки, точка, история ремонта)
- **ServicePoint** — точка обслуживания (адрес, координаты, регион, сеть, контакты, оборудование)
- **Client (Organization)** — организация (наименование, сеть, контакты, точки, ТТК)
- **SparePart** — запчасть [модуль неактивен] (артикул, название, цена, история цен, признак дорогостоящей)
- **AppUser** — пользователь (ФИО, телефон, email, роль, компания)
- **ChatMessage / ChatAttachment** — сообщения и файлы в чате заявки
- **Notification** — уведомления (Web Push)
- **AuditLog** — журнал аудита

## Правила и ограничения при написании кода
- **HTML `<table>` запрещён** — использовать div.table, Bootstrap cards, list-group, grid
- Все комментарии в коде на **русском языке**
- Контроллеры **тонкие**: ≤5 строк на метод (принял → вызвал сервис → вернул)
- MVC-контроллеры: `[HttpGet]`/`[HttpPost]` **без строки маршрута** (конвенция Controller/Action/Id)
- API-контроллеры: `[ApiController]` + `[Route("api/...")]` на классе
- DI регистрация через extension methods: `AddApplication()`, `AddInfrastructure()`
- CSS-переменные для тем (`data-theme="dark"` на `<html>`)
- Маппинг **ручной** (без AutoMapper) — extension methods в `MappingExtensions.cs`
- Переходы статусов заявок — через `TicketStatusTransitions` (единое место)
- Файлы: **без ограничений** по размеру и количеству
- SMS-код: не чаще 1 раз/мин, макс 5 попыток, блокировка 20 мин
- Сессия бессрочная (до явного выхода)
- BaseEntity: `Id`, `CreatedAt`, `UpdatedAt` (автоматически при SaveChanges)
- Namespace по папке: `ServiceDesk.{Layer}.{Folder}`
- Офлайн: Service Worker + IndexedDB, серверные изменения имеют приоритет
- Язык интерфейса — **только русский**
