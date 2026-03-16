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

    // Сбрасываем группы вложений при каждой перерисовке
    _chatAttachmentGroups = [];

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
            // Сохраняем вложения сообщения в data-атрибут для галереи
            var msgIndex = _chatAttachmentGroups.length;
            _chatAttachmentGroups.push(msg.attachments);

            attachmentsHtml = '<div class="chat-attachments d-flex flex-wrap gap-2 mt-1">' +
                msg.attachments.map(function (att, attIndex) {
                    var isImage = att.fileName.match(/\.(jpg|jpeg|png|gif|webp|bmp)$/i);
                    var isVideo = att.fileName.match(/\.(mp4|webm|mov|avi)$/i);

                    if (isImage) {
                        return '<img src="' + escapeHtml(att.filePath) + '" alt="' + escapeHtml(att.fileName) + '" ' +
                            'class="chat-attachment-img rounded" style="max-width:200px;max-height:150px;object-fit:cover;cursor:pointer;" ' +
                            'onclick="openGallery(' + msgIndex + ',' + attIndex + ')" />';
                    } else if (isVideo) {
                        return '<div class="position-relative d-inline-block" style="cursor:pointer;" ' +
                            'onclick="openGallery(' + msgIndex + ',' + attIndex + ')">' +
                            '<video src="' + escapeHtml(att.filePath) + '" ' +
                            'class="rounded" style="max-width:250px;max-height:180px;pointer-events:none;"></video>' +
                            '<div class="position-absolute top-50 start-50 translate-middle text-white bg-dark bg-opacity-50 rounded-circle d-flex align-items-center justify-content-center" style="width:40px;height:40px;">' +
                            '<i class="bi bi-play-fill fs-5"></i></div></div>';
                    } else {
                        return '<a href="javascript:void(0)" class="btn btn-sm btn-outline-secondary" ' +
                            'onclick="openGallery(' + msgIndex + ',' + attIndex + ')">' +
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

// Галерея вложений
var _chatAttachmentGroups = [];
var _galleryItems = [];
var _galleryIndex = 0;

// Открытие галереи на конкретном вложении
function openGallery(groupIndex, itemIndex) {
    _galleryItems = _chatAttachmentGroups[groupIndex] || [];
    _galleryIndex = itemIndex;
    renderGallerySlide();

    var modalEl = document.getElementById('attachmentModal');
    var modal = bootstrap.Modal.getInstance(modalEl) || new bootstrap.Modal(modalEl);
    modal.show();

    // Останавливаем видео при закрытии и убираем обработчик клавиш
    modalEl.addEventListener('hidden.bs.modal', function () {
        var video = document.getElementById('galleryContent').querySelector('video');
        if (video) video.pause();
        document.removeEventListener('keydown', _galleryKeyHandler);
    }, { once: true });

    // Навигация клавишами
    document.removeEventListener('keydown', _galleryKeyHandler);
    document.addEventListener('keydown', _galleryKeyHandler);
}

// Обработчик клавиш для галереи
function _galleryKeyHandler(e) {
    if (e.key === 'ArrowLeft') galleryNav(-1);
    if (e.key === 'ArrowRight') galleryNav(1);
}

// Навигация по галерее
function galleryNav(direction) {
    // Останавливаем текущее видео перед переключением
    var video = document.getElementById('galleryContent').querySelector('video');
    if (video) video.pause();

    _galleryIndex += direction;
    if (_galleryIndex < 0) _galleryIndex = _galleryItems.length - 1;
    if (_galleryIndex >= _galleryItems.length) _galleryIndex = 0;
    renderGallerySlide();
}

// Отрисовка текущего слайда
function renderGallerySlide() {
    var att = _galleryItems[_galleryIndex];
    var content = document.getElementById('galleryContent');
    var title = document.getElementById('attachmentModalTitle');
    var counter = document.getElementById('attachmentCounter');
    var prevBtn = document.getElementById('galleryPrev');
    var nextBtn = document.getElementById('galleryNext');

    title.textContent = att.fileName;
    counter.textContent = (_galleryIndex + 1) + ' / ' + _galleryItems.length;

    // Скрываем стрелки если одно вложение
    var showNav = _galleryItems.length > 1;
    prevBtn.style.display = showNav ? '' : 'none';
    nextBtn.style.display = showNav ? '' : 'none';
    counter.style.display = showNav ? '' : 'none';

    var isImage = att.fileName.match(/\.(jpg|jpeg|png|gif|webp|bmp)$/i);
    var isVideo = att.fileName.match(/\.(mp4|webm|mov|avi)$/i);

    if (isImage) {
        content.innerHTML = '<img src="' + escapeHtml(att.filePath) + '" alt="' + escapeHtml(att.fileName) + '" ' +
            'class="img-fluid rounded" style="max-height:75vh;" />';
    } else if (isVideo) {
        content.innerHTML = '<video src="' + escapeHtml(att.filePath) + '" controls autoplay ' +
            'class="rounded" style="max-width:100%;max-height:75vh;"></video>';
    } else {
        content.innerHTML = '<div class="py-4">' +
            '<i class="bi bi-file-earmark fs-1 text-muted"></i>' +
            '<p class="mt-2">' + escapeHtml(att.fileName) + '</p>' +
            '<a href="' + escapeHtml(att.filePath) + '" download class="btn btn-primary">' +
            '<i class="bi bi-download"></i> Скачать</a></div>';
    }
}

// Экранирование HTML для защиты от XSS
function escapeHtml(text) {
    const div = document.createElement('div');
    div.textContent = text;
    return div.innerHTML;
}
