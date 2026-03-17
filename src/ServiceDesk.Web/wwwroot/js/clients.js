'use strict';

/**
 * Модуль клиентов — клиентская логика для раздела /Clients
 */
const ClientsModule = (() => {

    /** Инициализация поиска с задержкой */
    function initSearch() {
        const searchInput = document.querySelector('input[name="Search"]');
        if (!searchInput) return;

        let debounceTimer;
        searchInput.addEventListener('input', () => {
            clearTimeout(debounceTimer);
            debounceTimer = setTimeout(() => {
                searchInput.closest('form')?.submit();
            }, 500);
        });
    }

    /** Инициализация маски ИНН (только цифры, макс 12) */
    function initInnMask() {
        const innInput = document.querySelector('input[name="Inn"]');
        if (!innInput) return;

        innInput.addEventListener('input', (e) => {
            e.target.value = e.target.value.replace(/\D/g, '').slice(0, 12);
        });
    }

    /** Инициализация подтверждений удаления */
    function initDeleteConfirmations() {
        document.querySelectorAll('[data-confirm]').forEach(btn => {
            btn.addEventListener('click', (e) => {
                if (!confirm(btn.dataset.confirm)) {
                    e.preventDefault();
                }
            });
        });
    }

    /** Общая инициализация */
    function init() {
        initSearch();
        initInnMask();
        initDeleteConfirmations();
    }

    // Запуск при загрузке страницы
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', init);
    } else {
        init();
    }

    return { init };
})();
