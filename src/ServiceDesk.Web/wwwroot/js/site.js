// Общая логика приложения

// Переключение сайдбара (мобильная версия)
function toggleSidebar() {
    const sidebar = document.getElementById('sidebar');
    sidebar.classList.toggle('show');
}

// Закрытие сайдбара при клике вне его (мобильная версия)
document.addEventListener('click', function (e) {
    const sidebar = document.getElementById('sidebar');
    const toggle = document.querySelector('.sidebar-toggle');

    if (sidebar && sidebar.classList.contains('show') &&
        !sidebar.contains(e.target) &&
        (!toggle || !toggle.contains(e.target))) {
        sidebar.classList.remove('show');
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
