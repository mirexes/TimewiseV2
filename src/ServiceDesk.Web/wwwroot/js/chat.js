// Логика чата заявки

let currentTicketId = null;

// Инициализация чата
function initChat(ticketId) {
    currentTicketId = ticketId;
    loadMessages();

    // Обработчик отправки формы
    const form = document.getElementById('chatForm');
    if (form) {
        form.addEventListener('submit', function (e) {
            e.preventDefault();
            sendMessage();
        });
    }

    // Обновляем сообщения каждые 10 секунд
    setInterval(loadMessages, 10000);
}

// Загрузка сообщений
async function loadMessages() {
    if (!currentTicketId) return;

    try {
        const response = await fetch('/api/chat/' + currentTicketId);
        if (response.ok) {
            const messages = await response.json();
            renderMessages(messages);
        }
    } catch (e) {
        // Молча обрабатываем ошибку
    }
}

// Отрисовка сообщений
function renderMessages(messages) {
    const container = document.getElementById('chatMessages');
    if (!container) return;

    if (messages.length === 0) {
        container.innerHTML = '<p class="text-muted text-center small">Нет сообщений</p>';
        return;
    }

    container.innerHTML = messages.map(function (msg) {
        const time = new Date(msg.createdAt).toLocaleString('ru-RU', {
            day: '2-digit', month: '2-digit',
            hour: '2-digit', minute: '2-digit'
        });

        return '<div class="chat-message">' +
            '<div class="sender">' + escapeHtml(msg.senderName) + '</div>' +
            '<div>' + escapeHtml(msg.text) + '</div>' +
            '<div class="time">' + time + '</div>' +
            '</div>';
    }).join('');

    // Прокручиваем к последнему сообщению
    container.scrollTop = container.scrollHeight;
}

// Отправка сообщения
async function sendMessage() {
    const input = document.getElementById('chatInput');
    const text = input.value.trim();
    if (!text || !currentTicketId) return;

    try {
        const response = await fetch('/api/chat/send', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({
                text: text,
                ticketId: currentTicketId
            })
        });

        if (response.ok) {
            input.value = '';
            loadMessages();
        }
    } catch (e) {
        alert('Ошибка отправки');
    }
}

// Экранирование HTML для защиты от XSS
function escapeHtml(text) {
    const div = document.createElement('div');
    div.textContent = text;
    return div.innerHTML;
}
