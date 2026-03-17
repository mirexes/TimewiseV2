// Логика работы с заявками (AJAX)

// Смена статуса заявки
async function updateStatus(ticketId, newStatus) {
    const comment = prompt('Комментарий (необязательно):');

    try {
        const response = await fetch('/api/tickets/status', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({
                ticketId: ticketId,
                newStatus: newStatus,
                comment: comment
            })
        });

        if (response.ok) {
            location.reload();
        } else {
            const error = await response.text();
            alert('Ошибка: ' + error);
        }
    } catch (e) {
        alert('Ошибка соединения');
    }
}

// Загрузка списка специалистов в выпадающий список
async function loadEngineers() {
    const select = document.getElementById('engineerSelect');
    if (!select) return;

    try {
        const response = await fetch('/api/tickets/engineers');
        if (!response.ok) {
            console.error('Ошибка загрузки специалистов: HTTP ' + response.status);
            return;
        }

        const engineers = await response.json();
        engineers.forEach(eng => {
            const option = document.createElement('option');
            option.value = eng.id;
            option.textContent = `${eng.fullName} (${eng.role})`;
            select.appendChild(option);
        });
    } catch (e) {
        console.error('Ошибка загрузки специалистов:', e);
    }
}

// Назначение специалиста на заявку
async function assignEngineer(ticketId) {
    const select = document.getElementById('engineerSelect');
    const engineerId = parseInt(select.value);

    if (!engineerId) {
        alert('Выберите специалиста');
        return;
    }

    try {
        const response = await fetch('/api/tickets/assign', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({
                ticketId: ticketId,
                engineerId: engineerId
            })
        });

        if (response.ok) {
            location.reload();
        } else {
            const error = await response.text();
            alert('Ошибка: ' + error);
        }
    } catch (e) {
        alert('Ошибка соединения');
    }
}

// Загружаем специалистов при наличии выпадающего списка
if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', loadEngineers);
} else {
    loadEngineers();
}
