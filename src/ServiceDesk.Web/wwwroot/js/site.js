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
function toggleNotifications() {
    // TODO: Реализовать выпадающую панель уведомлений
    updateNotificationCount();
}

// Обновляем счётчик каждые 30 секунд
if (document.getElementById('notificationCount')) {
    updateNotificationCount();
    setInterval(updateNotificationCount, 30000);
}
