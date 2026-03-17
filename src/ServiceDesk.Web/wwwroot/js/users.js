'use strict';

/**
 * Модуль управления формой пользователя.
 * Показывает/скрывает выбор клиента при роли «Менеджер клиента».
 */
(function () {
    const roleSelect = document.getElementById('roleSelect');
    const clientGroup = document.getElementById('clientSelectGroup');

    if (!roleSelect || !clientGroup) return;

    function toggleClientSelect() {
        // ManagerClient = 6
        const isManagerClient = roleSelect.value === 'ManagerClient' || roleSelect.value === '6';
        clientGroup.style.display = isManagerClient ? '' : 'none';
    }

    roleSelect.addEventListener('change', toggleClientSelect);

    // Инициализация при загрузке страницы
    toggleClientSelect();
})();
