// Логика чата заявки (Telegram-стиль)

let currentTicketId = null;
let currentUserId = null;
let _chatAttachmentGroups = [];
let _galleryItems = [];
let _galleryIndex = 0;
let _emojiPickerReady = false;
let _lastMessageCount = 0;
let _isAtBottom = true;

// === Набор эмодзи по категориям ===
const EMOJI_CATEGORIES = [
    {
        icon: '😀', name: 'Смайлы',
        emojis: [
            '😀','😃','😄','😁','😆','😅','🤣','😂','🙂','😊',
            '😇','🥰','😍','🤩','😘','😗','😋','😛','😜','🤪',
            '😝','🤑','🤗','🤭','🤫','🤔','😐','😑','😶','😏',
            '😒','🙄','😬','😮‍💨','🤥','😌','😔','😪','🤤','😴',
            '😷','🤒','🤕','🤢','🤮','🥵','🥶','🥴','😵','🤯',
            '🤠','🥳','🥸','😎','🤓','🧐','😕','😟','🙁','😮',
            '😯','😲','😳','🥺','😦','😧','😨','😰','😥','😢',
            '😭','😱','😖','😣','😞','😓','😩','😫','🥱','😤',
            '😡','😠','🤬','😈','👿','💀','☠️','💩','🤡','👹',
            '👺','👻','👽','👾','🤖'
        ]
    },
    {
        icon: '👋', name: 'Жесты',
        emojis: [
            '👋','🤚','🖐️','✋','🖖','👌','🤌','🤏','✌️','🤞',
            '🤟','🤘','🤙','👈','👉','👆','🖕','👇','☝️','👍',
            '👎','✊','👊','🤛','🤜','👏','🙌','👐','🤲','🤝',
            '🙏','✍️','💅','🤳','💪','🦾','🦿','🦵','🦶','👂',
            '🦻','👃','🧠','🫀','🫁','🦷','🦴','👀','👁️','👅','👄'
        ]
    },
    {
        icon: '❤️', name: 'Символы',
        emojis: [
            '❤️','🧡','💛','💚','💙','💜','🖤','🤍','🤎','💔',
            '❣️','💕','💞','💓','💗','💖','💘','💝','💟','☮️',
            '✝️','☪️','🕉️','☸️','✡️','🔯','🕎','☯️','☦️','🛐',
            '⛎','♈','♉','♊','♋','♌','♍','♎','♏','♐',
            '♑','♒','♓','🆔','⚛️','🉑','☢️','☣️','📴','📳',
            '🈶','🈚','🈸','🈺','🈷️','✴️','🆚','💮','🉐','㊙️',
            '㊗️','🈴','🈵','🈹','🈲','🅰️','🅱️','🆎','🆑','🅾️',
            '🆘','❌','⭕','🛑','⛔','📛','🚫','💯','💢','♨️',
            '🚷','🚱','🚳','🚭','🚮','✅','☑️','✔️','❎','➕','➖','➗','➰','➿','〽️','✳️','✴️','❇️','‼️','⁉️','❓','❔','❕','❗','〰️','©️','®️','™️'
        ]
    },
    {
        icon: '🐶', name: 'Животные',
        emojis: [
            '🐶','🐱','🐭','🐹','🐰','🦊','🐻','🐼','🐻‍❄️','🐨',
            '🐯','🦁','🐮','🐷','🐽','🐸','🐵','🙈','🙉','🙊',
            '🐒','🐔','🐧','🐦','🐤','🐣','🐥','🦆','🦅','🦉',
            '🦇','🐺','🐗','🐴','🦄','🐝','🪱','🐛','🦋','🐌',
            '🐞','🐜','🪰','🪲','🪳','🦟','🦗','🕷️','🕸️','🦂',
            '🐢','🐍','🦎','🦖','🦕','🐙','🦑','🦐','🦞','🦀',
            '🐡','🐠','🐟','🐬','🐳','🐋','🦈','🦭','🐊','🐅',
            '🐆','🦓','🦍','🦧','🐘','🦛','🦏','🐪','🐫','🦒',
            '🦘','🦬','🐃','🐂','🐄','🐎','🐖','🐏','🐑','🦙',
            '🐐','🦌','🐕','🐩','🦮'
        ]
    },
    {
        icon: '🍔', name: 'Еда',
        emojis: [
            '🍏','🍎','🍐','🍊','🍋','🍌','🍉','🍇','🍓','🫐',
            '🍈','🍒','🍑','🥭','🍍','🥥','🥝','🍅','🍆','🥑',
            '🥦','🥬','🥒','🌶️','🫑','🌽','🥕','🫒','🧄','🧅',
            '🥔','🍠','🥐','🥯','🍞','🥖','🥨','🧀','🥚','🍳',
            '🧈','🥞','🧇','🥓','🥩','🍗','🍖','🦴','🌭','🍔',
            '🍟','🍕','🫓','🥪','🥙','🧆','🌮','🌯','🫔','🥗',
            '🥘','🫕','🥫','🍝','🍜','🍲','🍛','🍣','🍱','🥟',
            '🦪','🍤','🍙','🍚','🍘','🍥','🥠','🥮','🍡','🍧',
            '🍨','🍦','🥧','🧁','🍰','🎂','🍮','🍭','🍬','🍫',
            '🍿','🍩','🍪','🌰','🥜','🍯'
        ]
    },
    {
        icon: '⚽', name: 'Активности',
        emojis: [
            '⚽','🏀','🏈','⚾','🥎','🎾','🏐','🏉','🥏','🎱',
            '🪀','🏓','🏸','🏒','🏑','🥍','🏏','🪃','🥅','⛳',
            '🪁','🏹','🎣','🤿','🥊','🥋','🎽','🛹','🛼','🛷',
            '⛸️','🥌','🎿','⛷️','🏂','🪂','🏋️','🤼','🤸','⛹️',
            '🤺','🤾','🏌️','🏇','🧘','🏄','🏊','🤽','🚣','🧗',
            '🚵','🚴','🏆','🥇','🥈','🥉','🏅','🎖️','🏵️','🎗️',
            '🎫','🎟️','🎪','🎭','🎨','🎬','🎤','🎧','🎼','🎹',
            '🥁','🪘','🎷','🎺','🪗','🎸','🪕','🎻','🎲','♟️',
            '🎯','🎳','🎮','🕹️','🎰'
        ]
    }
];

// Инициализация чата
function initChat(ticketId, userId) {
    currentTicketId = ticketId;
    currentUserId = userId;

    const container = document.getElementById('chatMessages');
    // Отслеживаем положение скролла
    if (container) {
        container.addEventListener('scroll', function () {
            _isAtBottom = container.scrollHeight - container.scrollTop - container.clientHeight < 40;
        });
    }

    loadMessages();
    setupChatForm();
    setupEmojiPicker();
    setupTextareaAutoResize();

    // Обновляем сообщения каждые 5 секунд
    setInterval(loadMessages, 5000);
}

// Настройка формы отправки
function setupChatForm() {
    const form = document.getElementById('chatForm');
    if (form) {
        form.addEventListener('submit', function (e) {
            e.preventDefault();
            sendMessage();
        });
    }

    // Отправка по Enter (Shift+Enter для новой строки)
    const input = document.getElementById('chatInput');
    if (input) {
        input.addEventListener('keydown', function (e) {
            if (e.key === 'Enter' && !e.shiftKey) {
                e.preventDefault();
                sendMessage();
            }
        });
    }
}

// Автоматическое изменение высоты textarea
function setupTextareaAutoResize() {
    const textarea = document.getElementById('chatInput');
    if (!textarea) return;

    textarea.addEventListener('input', function () {
        this.style.height = 'auto';
        this.style.height = Math.min(this.scrollHeight, 120) + 'px';
    });
}

// === Панель эмодзи ===
function setupEmojiPicker() {
    const toggle = document.getElementById('emojiToggle');
    const picker = document.getElementById('emojiPicker');
    if (!toggle || !picker) return;

    toggle.addEventListener('click', function () {
        if (!_emojiPickerReady) {
            buildEmojiPicker();
            _emojiPickerReady = true;
        }
        picker.classList.toggle('open');
        // Меняем иконку кнопки
        const icon = toggle.querySelector('i');
        if (picker.classList.contains('open')) {
            icon.className = 'bi bi-keyboard';
        } else {
            icon.className = 'bi bi-emoji-smile';
        }
    });
}

function buildEmojiPicker() {
    const picker = document.getElementById('emojiPicker');

    // Вкладки категорий
    var tabsHtml = '<div class="emoji-picker-tabs">';
    EMOJI_CATEGORIES.forEach(function (cat, i) {
        tabsHtml += '<button type="button" data-cat="' + i + '"' +
            (i === 0 ? ' class="active"' : '') + ' title="' + escapeHtml(cat.name) + '">' +
            cat.icon + '</button>';
    });
    tabsHtml += '</div>';

    // Сетка эмодзи
    var gridHtml = '<div class="emoji-picker-grid" id="emojiGrid"></div>';
    picker.innerHTML = tabsHtml + gridHtml;

    // Отрисовываем первую категорию
    renderEmojiCategory(0);

    // Обработчики вкладок
    picker.querySelectorAll('.emoji-picker-tabs button').forEach(function (btn) {
        btn.addEventListener('click', function () {
            picker.querySelectorAll('.emoji-picker-tabs button').forEach(function (b) {
                b.classList.remove('active');
            });
            btn.classList.add('active');
            renderEmojiCategory(parseInt(btn.dataset.cat));
        });
    });
}

function renderEmojiCategory(catIndex) {
    var grid = document.getElementById('emojiGrid');
    if (!grid) return;

    var emojis = EMOJI_CATEGORIES[catIndex].emojis;
    grid.innerHTML = emojis.map(function (emoji) {
        return '<button type="button" data-emoji="' + emoji + '">' + emoji + '</button>';
    }).join('');

    // Обработчики клика по эмодзи
    grid.querySelectorAll('button').forEach(function (btn) {
        btn.addEventListener('click', function () {
            insertEmoji(btn.dataset.emoji);
        });
    });
}

function insertEmoji(emoji) {
    var textarea = document.getElementById('chatInput');
    if (!textarea) return;

    var start = textarea.selectionStart;
    var end = textarea.selectionEnd;
    var text = textarea.value;
    textarea.value = text.substring(0, start) + emoji + text.substring(end);
    textarea.selectionStart = textarea.selectionEnd = start + emoji.length;
    textarea.focus();

    // Обновляем высоту
    textarea.dispatchEvent(new Event('input'));
}

// === Загрузка и отрисовка сообщений ===
async function loadMessages() {
    if (!currentTicketId) return;

    try {
        var response = await fetch('/api/chat/' + currentTicketId);
        if (response.ok) {
            var messages = await response.json();
            // Перерисовываем только если изменилось количество
            if (messages.length !== _lastMessageCount) {
                _lastMessageCount = messages.length;
                renderMessages(messages);
            }
        }
    } catch (e) {
        // Молча обрабатываем ошибку сети
    }
}

function renderMessages(messages) {
    var container = document.getElementById('chatMessages');
    if (!container) return;

    _chatAttachmentGroups = [];

    if (messages.length === 0) {
        container.innerHTML =
            '<div class="chat-empty">' +
            '<i class="bi bi-chat-text"></i>' +
            '<span>Нет сообщений. Напишите первое!</span>' +
            '</div>';
        return;
    }

    var html = '';
    var lastDate = '';

    messages.forEach(function (msg, index) {
        var msgDate = new Date(msg.createdAt);
        var dateStr = formatDate(msgDate);

        // Разделитель дат
        if (dateStr !== lastDate) {
            html += '<div class="chat-date-separator">' + dateStr + '</div>';
            lastDate = dateStr;
        }

        var isOwn = msg.senderId === currentUserId;
        var time = msgDate.toLocaleTimeString('ru-RU', { hour: '2-digit', minute: '2-digit' });

        // Вложения
        var attachmentsHtml = '';
        if (msg.attachments && msg.attachments.length > 0) {
            var msgIndex = _chatAttachmentGroups.length;
            _chatAttachmentGroups.push(msg.attachments);

            attachmentsHtml = '<div class="chat-attachments d-flex flex-wrap gap-2 mt-1">' +
                msg.attachments.map(function (att, attIndex) {
                    var isImage = att.fileName.match(/\.(jpg|jpeg|png|gif|webp|bmp)$/i);
                    var isVideo = att.fileName.match(/\.(mp4|webm|mov|avi)$/i);

                    if (isImage) {
                        return '<img src="' + escapeHtml(att.filePath) + '" alt="' + escapeHtml(att.fileName) + '" ' +
                            'class="chat-attachment-img" style="max-width:200px;max-height:150px;object-fit:cover;" ' +
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

        // Галочки прочтения для своих сообщений
        var readCheck = '';
        if (isOwn) {
            readCheck = msg.isRead
                ? '<span class="read-check read" title="Прочитано"><i class="bi bi-check2-all"></i></span>'
                : '<span class="read-check" title="Доставлено"><i class="bi bi-check2"></i></span>';
        }

        // Текст с обнаружением ссылок
        var textHtml = linkify(escapeHtml(msg.text || ''));

        html +=
            '<div class="chat-message ' + (isOwn ? 'own' : 'incoming') + '">' +
            (isOwn ? '' : '<div class="sender">' + escapeHtml(msg.senderName) + '</div>') +
            '<div class="msg-text">' + textHtml +
            '<span class="msg-meta">' + time + readCheck + '</span>' +
            '</div>' +
            attachmentsHtml +
            '</div>';
    });

    container.innerHTML = html;

    // Прокрутка к последнему сообщению
    if (_isAtBottom) {
        container.scrollTop = container.scrollHeight;
    }
}

// Форматирование даты для разделителя
function formatDate(date) {
    var today = new Date();
    var yesterday = new Date();
    yesterday.setDate(yesterday.getDate() - 1);

    if (date.toDateString() === today.toDateString()) return 'Сегодня';
    if (date.toDateString() === yesterday.toDateString()) return 'Вчера';

    return date.toLocaleDateString('ru-RU', { day: 'numeric', month: 'long', year: 'numeric' });
}

// Обнаружение URL в тексте
function linkify(text) {
    var urlRegex = /(https?:\/\/[^\s<]+)/g;
    return text.replace(urlRegex, function (url) {
        return '<a href="' + url + '" target="_blank" rel="noopener noreferrer">' + url + '</a>';
    });
}

// === Отправка сообщения ===
async function sendMessage() {
    var input = document.getElementById('chatInput');
    var text = input.value.trim();
    if (!text || !currentTicketId) return;

    // Закрываем эмодзи-панель
    var picker = document.getElementById('emojiPicker');
    if (picker) picker.classList.remove('open');
    var emojiIcon = document.querySelector('#emojiToggle i');
    if (emojiIcon) emojiIcon.className = 'bi bi-emoji-smile';

    // Очищаем поле и сбрасываем высоту
    input.value = '';
    input.style.height = 'auto';

    try {
        var response = await fetch('/api/chat/send', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({
                text: text,
                ticketId: currentTicketId
            })
        });

        if (response.ok) {
            _lastMessageCount = 0; // Принудительная перерисовка
            _isAtBottom = true;
            loadMessages();
        }
    } catch (e) {
        // Возвращаем текст при ошибке
        input.value = text;
        input.dispatchEvent(new Event('input'));
    }
}

// === Галерея вложений ===
function openGallery(groupIndex, itemIndex) {
    _galleryItems = _chatAttachmentGroups[groupIndex] || [];
    _galleryIndex = itemIndex;
    renderGallerySlide();

    var modalEl = document.getElementById('attachmentModal');
    var modal = bootstrap.Modal.getInstance(modalEl) || new bootstrap.Modal(modalEl);
    modal.show();

    modalEl.addEventListener('hidden.bs.modal', function () {
        var video = document.getElementById('galleryContent').querySelector('video');
        if (video) video.pause();
        document.removeEventListener('keydown', _galleryKeyHandler);
    }, { once: true });

    document.removeEventListener('keydown', _galleryKeyHandler);
    document.addEventListener('keydown', _galleryKeyHandler);
}

function _galleryKeyHandler(e) {
    if (e.key === 'ArrowLeft') galleryNav(-1);
    if (e.key === 'ArrowRight') galleryNav(1);
}

function galleryNav(direction) {
    var video = document.getElementById('galleryContent').querySelector('video');
    if (video) video.pause();

    _galleryIndex += direction;
    if (_galleryIndex < 0) _galleryIndex = _galleryItems.length - 1;
    if (_galleryIndex >= _galleryItems.length) _galleryIndex = 0;
    renderGallerySlide();
}

function renderGallerySlide() {
    var att = _galleryItems[_galleryIndex];
    var content = document.getElementById('galleryContent');
    var title = document.getElementById('attachmentModalTitle');
    var counter = document.getElementById('attachmentCounter');
    var prevBtn = document.getElementById('galleryPrev');
    var nextBtn = document.getElementById('galleryNext');

    title.textContent = att.fileName;
    counter.textContent = (_galleryIndex + 1) + ' / ' + _galleryItems.length;

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
    if (!text) return '';
    var div = document.createElement('div');
    div.textContent = text;
    return div.innerHTML;
}
