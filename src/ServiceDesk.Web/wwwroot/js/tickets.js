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
            // Перезагружаем страницу для обновления данных
            location.reload();
        } else {
            const error = await response.text();
            alert('Ошибка: ' + error);
        }
    } catch (e) {
        alert('Ошибка соединения');
    }
}
