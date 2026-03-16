# Создание Razor View для ServiceDesk

Создай Razor View (.cshtml) для указанной страницы/компонента.

## Входные данные
Пользователь указывает: что нужно создать (список, карточка, форма создания, и т.д.)

## Общие правила для всех Views

### Запрещено
- HTML-тег `<table>` — использовать `div.table`, Bootstrap cards, list-group, grid
- jQuery — только Vanilla JavaScript + Fetch API
- Inline стили — использовать CSS-переменные из `_variables.css`

### Обязательно
- Адаптивная вёрстка (десктоп + мобильные)
- CSS-переменные для цветов: `var(--bg-card)`, `var(--text-primary)` и т.д.
- Bootstrap 5.3 классы
- Bootstrap Icons (`bi bi-*`) или Font Awesome
- Все тексты на **русском языке**
- `asp-` tag helpers для форм и ссылок

### Десктоп vs Мобильные
- Десктоп: list-group или div.table для списков, sidebar меню слева
- Мобильные: Bootstrap cards, нижний навбар (макс 5 кнопок)

## Типы Views

### 1. Список (Index.cshtml)
```html
@model PagedResultDto<{Entity}ListDto>

<!-- Панель фильтров -->
@await Component.InvokeAsync("FilterPanel", new { ... })

<!-- Список -->
<div class="row g-3">
    @foreach (var item in Model.Items)
    {
        <div class="col-12">
            @await Component.InvokeAsync("TicketCard", item)
        </div>
    }
</div>

<!-- Пагинация -->
@await Html.PartialAsync("_Pagination", Model)
```

### 2. Карточка деталей (Details.cshtml)
- Все данные сущности
- Кнопки действий (смена статуса) по роли пользователя
- Чат (если заявка): `@await Component.InvokeAsync("ChatWidget", ...)`
- Кнопка навигации (Яндекс.Карты) для заявок с адресом

### 3. Форма создания (Create.cshtml)
```html
@model Create{Entity}Dto

<form asp-action="Create" method="post" enctype="multipart/form-data">
    @Html.AntiForgeryToken()
    <!-- Поля формы с Bootstrap классами -->
    <div class="mb-3">
        <label asp-for="Name" class="form-label">Название</label>
        <input asp-for="Name" class="form-control" />
        <span asp-validation-for="Name" class="text-danger"></span>
    </div>
    <button type="submit" class="btn btn-primary">Создать</button>
</form>
```

### 4. ViewComponent (переиспользуемый блок)
Файл компонента: `ServiceDesk.Web/ViewComponents/{Name}ViewComponent.cs`
Файл view: `ServiceDesk.Web/Views/Shared/Components/{Name}/Default.cshtml`

## Цветовые бейджи статусов
```html
<span class="badge" style="background: var(--status-new)">Новая</span>
<span class="badge" style="background: var(--status-processing)">В обработке</span>
<span class="badge" style="background: var(--status-done)">Выполнена</span>
<span class="badge" style="background: var(--status-cancelled)">Отменена</span>
<span class="badge" style="background: var(--status-waiting)">Ожидание</span>
```

## AJAX-операции (без перезагрузки страницы)
```javascript
// Пример: смена статуса заявки
async function updateStatus(ticketId, newStatus) {
    const response = await fetch('/api/tickets/status', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]').value
        },
        body: JSON.stringify({ ticketId, newStatus })
    });
    const result = await response.json();
    if (result.success) {
        // Обновить UI
    }
}
```

$ARGUMENTS
