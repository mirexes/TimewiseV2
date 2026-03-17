// Общая логика приложения

// Переключение сайдбара (мобильная версия)
function toggleSidebar() {
    const sidebar = document.getElementById('sidebar');
    const overlay = document.getElementById('sidebarOverlay');
    if (!sidebar) return;

    const isOpen = sidebar.classList.toggle('show');
    if (overlay) {
        overlay.classList.toggle('show', isOpen);
    }
    // Блокируем скролл фона при открытом сайдбаре
    document.body.style.overflow = isOpen ? 'hidden' : '';
}

// Закрытие сайдбара при клике по ссылке внутри (мобильная версия)
document.addEventListener('DOMContentLoaded', function () {
    const sidebar = document.getElementById('sidebar');
    if (sidebar) {
        sidebar.addEventListener('click', function (e) {
            if (e.target.closest('.nav-link')) {
                sidebar.classList.remove('show');
                var overlay = document.getElementById('sidebarOverlay');
                if (overlay) overlay.classList.remove('show');
                document.body.style.overflow = '';
            }
        });
    }
});

// Обновление счётчика уведомлений
async function updateNotificationCount() {
    try {
        const response = await fetch('/api/notifications/count');
        if (response.ok) {
            const data = await response.json();
            const badge = document.getElementById('notificationCount');
            if (badge) {
                if (data.count > 0) {
                    badge.textContent = data.count;
                    badge.classList.remove('d-none');
                } else {
                    badge.classList.add('d-none');
                }
            }
        }
    } catch (e) {
        // Молча обрабатываем ошибку
    }
}

// Переключение панели уведомлений
async function toggleNotifications() {
    const panel = document.getElementById('notificationPanel');
    if (!panel) return;

    const isOpen = !panel.classList.contains('d-none');
    if (isOpen) {
        panel.classList.add('d-none');
        return;
    }

    // Загружаем список уведомлений и показываем панель
    panel.classList.remove('d-none');
    await loadNotifications();
}

// Загрузка списка уведомлений
async function loadNotifications() {
    const list = document.getElementById('notificationList');
    if (!list) return;

    try {
        const response = await fetch('/api/notifications');
        if (!response.ok) return;

        const data = await response.json();
        if (!data.length) {
            list.innerHTML = '<div class="notification-empty"><i class="bi bi-bell-slash"></i><span>Нет уведомлений</span></div>';
            return;
        }

        list.innerHTML = data.map(n => `
            <a href="${n.url || '#'}" class="notification-item ${n.isRead ? '' : 'unread'}" data-id="${n.id}">
                <div class="notification-item-title">${escapeHtml(n.title)}</div>
                <div class="notification-item-message">${escapeHtml(n.message)}</div>
                <div class="notification-item-time">${n.createdAt}</div>
            </a>
        `).join('');
    } catch (e) {
        // Молча обрабатываем ошибку
    }
}

// Пометить все уведомления как прочитанные
async function markAllNotificationsRead() {
    try {
        const response = await fetch('/api/notifications/read-all', { method: 'POST' });
        if (response.ok) {
            // Обновляем счётчик и список
            updateNotificationCount();
            const items = document.querySelectorAll('.notification-item.unread');
            items.forEach(item => item.classList.remove('unread'));
        }
    } catch (e) {
        // Молча обрабатываем ошибку
    }
}

// Экранирование HTML
function escapeHtml(text) {
    const div = document.createElement('div');
    div.textContent = text;
    return div.innerHTML;
}

// Закрытие панели при клике вне неё
document.addEventListener('click', function (e) {
    const wrapper = document.getElementById('notificationWrapper');
    const panel = document.getElementById('notificationPanel');
    if (wrapper && panel && !wrapper.contains(e.target)) {
        panel.classList.add('d-none');
    }
});

// Обновляем счётчик каждые 30 секунд
if (document.getElementById('notificationCount')) {
    updateNotificationCount();
    setInterval(updateNotificationCount, 30000);
}
