// Переключатель тем (светлая/тёмная)
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
        document.documentElement.setAttribute('data-bs-theme', theme);
        localStorage.setItem(STORAGE_KEY, theme);
    }

    // Применяем тему при загрузке (до отрисовки, чтобы не мерцало)
    applyTheme(getPreferredTheme());

    // Глобальная функция для кнопки
    window.toggleTheme = function () {
        const current = document.documentElement.getAttribute('data-theme');
        applyTheme(current === 'dark' ? 'light' : 'dark');
    };
})();
