# Справочник: бизнес-логика заявок ServiceDesk

Используй этот справочник при работе с заявками (Ticket). Здесь описаны все правила переходов статусов, ролевая модель и бизнес-процессы.

## 11 статусов заявки

| Код | Enum | Название (RU) | Описание |
|-----|------|---------------|----------|
| 0 | New | Новая | Создана, ожидает обработки |
| 1 | Processed | Обработана | Логист уточнил неисправность |
| 2 | CompletedRemotely | Выполнена дистанционно | Решена удалённо |
| 3 | PartsApproval | Согласование запчастей | Нужна дорогостоящая запчасть |
| 4 | RepairApproved | Ремонт согласован | Запчасти одобрены |
| 5 | InProgress | В работе | Специалист назначен |
| 6 | EngineerEnRoute | Техник в пути | Специалист едет на объект |
| 7 | Execution | Выполнение | Работы в процессе |
| 8 | Completed | Выполнена полностью | АВР прикреплён |
| 9 | ContinuationRequired | Требуется продолжение | Нужен повторный выезд |
| 10 | Cancelled | Отменена | Из любого статуса |

## Матрица переходов (TicketStatusTransitions)

```
New → Processed (Логист)
New → Cancelled (Менеджер TW / Модератор)

Processed → CompletedRemotely (Логист)
Processed → PartsApproval (Логист / Менеджер TW)
Processed → InProgress (Логист)
Processed → Cancelled (Менеджер TW / Модератор)

PartsApproval → RepairApproved (Менеджер TW / Менеджер клиента)
PartsApproval → Cancelled (Менеджер TW / Модератор)

RepairApproved → InProgress (Логист)
RepairApproved → Cancelled (Менеджер TW / Модератор)

InProgress → EngineerEnRoute (Техник / Инженер)
InProgress → Cancelled (Менеджер TW / Модератор)

EngineerEnRoute → Execution (Техник / Инженер)

Execution → Completed (Техник / Инженер)
Execution → ContinuationRequired (Техник / Инженер)

ContinuationRequired → InProgress (Логист)
ContinuationRequired → PartsApproval (Логист / Менеджер TW)
ContinuationRequired → Cancelled (Менеджер TW / Модератор)
```

## Кто может менять статус (по ролям)

| Роль | Может устанавливать статусы |
|------|-----------------------------|
| Техник / Инженер | EngineerEnRoute, Execution, Completed, ContinuationRequired |
| Главный инженер | То же + согласование запчастей |
| Логист | Processed, CompletedRemotely, InProgress (назначение), PartsApproval |
| Менеджер TW | PartsApproval, Cancelled |
| Менеджер клиента | RepairApproved (согласование запчастей) |
| Модератор | Любой статус |

## Типы заявок (TicketType)
- **Repair** (Ремонт) — обязательно: адрес, контакты, описание неисправности, сроки
- **Maintenance** (Обслуживание) — обязательно: адрес, контакты, описание работ, сроки
- **Delivery** (Поставка) — [модуль неактивен]

## Приоритеты (TicketPriority)
- Low (Низкий)
- Planned (Плановая)
- Medium (Средний)
- High (Высокий)
- или конкретная дата выполнения (Deadline)

## При смене статуса обязательно:
1. Проверить допустимость перехода через `TicketStatusTransitions.IsAllowed(from, to)`
2. Проверить права роли текущего пользователя
3. Залогировать через `IAuditService.LogAsync()`
4. Отправить уведомление через `INotificationService.OnTicketStatusChangedAsync()`
5. Зафиксировать время: `WorkStartedAt` (при Execution), `WorkCompletedAt` (при Completed/CompletedRemotely)

## Согласование запчастей (упрощённый режим, модуль запчастей неактивен)
Пользователь нажимает кнопку «Согласовано» → заявка переходит в RepairApproved.
Полный процесс: Менеджер TW инициирует → Менеджер клиента утверждает/отклоняет.
Автоматической отмены нет — решение за модератором.

## АВР (акт выполненных работ)
- Обязателен при закрытии заявки (Completed)
- Прикрепляется как фото документа
- При ContinuationRequired — промежуточный АВР

$ARGUMENTS
