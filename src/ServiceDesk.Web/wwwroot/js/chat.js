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

        var attachmentsHtml = '';
        if (msg.attachments && msg.attachments.length > 0) {
            attachmentsHtml = '<div class="chat-attachments d-flex flex-wrap gap-2 mt-1">' +
                msg.attachments.map(function (att) {
                    var isImage = att.fileName.match(/\.(jpg|jpeg|png|gif|webp|bmp)$/i);
                    var isVideo = att.fileName.match(/\.(mp4|webm|mov|avi)$/i);

                    if (isImage) {
                        return '<a href="' + escapeHtml(att.filePath) + '" target="_blank">' +
                            '<img src="' + escapeHtml(att.filePath) + '" alt="' + escapeHtml(att.fileName) + '" ' +
                            'class="chat-attachment-img rounded" style="max-width:200px;max-height:150px;object-fit:cover;cursor:pointer;" />' +
                            '</a>';
                    } else if (isVideo) {
                        return '<video src="' + escapeHtml(att.filePath) + '" controls ' +
                            'class="rounded" style="max-width:250px;max-height:180px;"></video>';
                    } else {
                        return '<a href="' + escapeHtml(att.filePath) + '" target="_blank" class="btn btn-sm btn-outline-secondary">' +
                            '<i class="bi bi-paperclip"></i> ' + escapeHtml(att.fileName) +
                            '</a>';
                    }
                }).join('') +
                '</div>';
        }

        return '<div class="chat-message">' +
            '<div class="sender">' + escapeHtml(msg.senderName) + '</div>' +
            '<div>' + escapeHtml(msg.text) + '</div>' +
            attachmentsHtml +
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
