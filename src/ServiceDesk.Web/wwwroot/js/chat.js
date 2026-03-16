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
                        return '<img src="' + escapeHtml(att.filePath) + '" alt="' + escapeHtml(att.fileName) + '" ' +
                            'class="chat-attachment-img rounded" style="max-width:200px;max-height:150px;object-fit:cover;cursor:pointer;" ' +
                            'onclick="openAttachment(\'' + escapeHtml(att.filePath) + '\', \'' + escapeHtml(att.fileName) + '\', \'image\')" />';
                    } else if (isVideo) {
                        return '<div class="position-relative d-inline-block" style="cursor:pointer;" ' +
                            'onclick="openAttachment(\'' + escapeHtml(att.filePath) + '\', \'' + escapeHtml(att.fileName) + '\', \'video\')">' +
                            '<video src="' + escapeHtml(att.filePath) + '" ' +
                            'class="rounded" style="max-width:250px;max-height:180px;pointer-events:none;"></video>' +
                            '<div class="position-absolute top-50 start-50 translate-middle text-white bg-dark bg-opacity-50 rounded-circle d-flex align-items-center justify-content-center" style="width:40px;height:40px;">' +
                            '<i class="bi bi-play-fill fs-5"></i></div></div>';
                    } else {
                        return '<a href="javascript:void(0)" class="btn btn-sm btn-outline-secondary" ' +
                            'onclick="openAttachment(\'' + escapeHtml(att.filePath) + '\', \'' + escapeHtml(att.fileName) + '\', \'file\')">' +
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

// Открытие вложения в модальном окне
function openAttachment(filePath, fileName, type) {
    var title = document.getElementById('attachmentModalTitle');
    var body = document.getElementById('attachmentModalBody');
    title.textContent = fileName;

    if (type === 'image') {
        body.innerHTML = '<img src="' + escapeHtml(filePath) + '" alt="' + escapeHtml(fileName) + '" ' +
            'class="img-fluid rounded" style="max-height:80vh;" />';
    } else if (type === 'video') {
        body.innerHTML = '<video src="' + escapeHtml(filePath) + '" controls autoplay ' +
            'class="rounded" style="max-width:100%;max-height:80vh;"></video>';
    } else {
        body.innerHTML = '<div class="py-4">' +
            '<i class="bi bi-file-earmark fs-1 text-muted"></i>' +
            '<p class="mt-2">' + escapeHtml(fileName) + '</p>' +
            '<a href="' + escapeHtml(filePath) + '" download class="btn btn-primary">' +
            '<i class="bi bi-download"></i> Скачать</a></div>';
    }

    var modal = new bootstrap.Modal(document.getElementById('attachmentModal'));
    modal.show();

    // Останавливаем видео при закрытии модалки
    document.getElementById('attachmentModal').addEventListener('hidden.bs.modal', function () {
        var video = body.querySelector('video');
        if (video) video.pause();
    }, { once: true });
}

// Экранирование HTML для защиты от XSS
function escapeHtml(text) {
    const div = document.createElement('div');
    div.textContent = text;
    return div.innerHTML;
}
