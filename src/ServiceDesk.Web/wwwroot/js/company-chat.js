// Логика группового чата компании (на основе чата заявки)

let _ccChatId = null;
let _ccUserId = null;
let _ccAttachmentGroups = [];
let _ccGalleryItems = [];
let _ccGalleryIndex = 0;
let _ccEmojiPickerReady = false;
let _ccLastMessageCount = 0;
let _ccIsAtBottom = true;
let _ccSelectedFiles = [];
let _ccReplyToMessage = null;

// Цвета для аватарок (по ID пользователя)
const CC_AVATAR_COLORS = [
    '#e17076', '#7bc862', '#e5ca77', '#65aadd',
    '#a695e7', '#ee7aae', '#6ec9cb', '#faa774'
];

// === Набор эмодзи по категориям ===
const CC_EMOJI_CATEGORIES = [
    {
        icon: '\u{1F600}', name: 'Смайлы',
        emojis: [
            '\u{1F600}','\u{1F603}','\u{1F604}','\u{1F601}','\u{1F606}','\u{1F605}','\u{1F923}','\u{1F602}','\u{1F642}','\u{1F60A}',
            '\u{1F607}','\u{1F970}','\u{1F60D}','\u{1F929}','\u{1F618}','\u{1F617}','\u{1F60B}','\u{1F61B}','\u{1F61C}','\u{1F92A}',
            '\u{1F61D}','\u{1F911}','\u{1F917}','\u{1F92D}','\u{1F92B}','\u{1F914}','\u{1F610}','\u{1F611}','\u{1F636}','\u{1F60F}',
            '\u{1F612}','\u{1F644}','\u{1F62C}','\u{1F62E}\u200D\u{1F4A8}','\u{1F925}','\u{1F60C}','\u{1F614}','\u{1F62A}','\u{1F924}','\u{1F634}',
            '\u{1F637}','\u{1F912}','\u{1F915}','\u{1F922}','\u{1F92E}','\u{1F975}','\u{1F976}','\u{1F974}','\u{1F635}','\u{1F92F}'
        ]
    },
    {
        icon: '\u{1F44B}', name: 'Жесты',
        emojis: [
            '\u{1F44B}','\u{1F91A}','\u{1F590}\uFE0F','\u270B','\u{1F596}','\u{1F44C}','\u{1F90C}','\u{1F90F}','\u270C\uFE0F','\u{1F91E}',
            '\u{1F91F}','\u{1F918}','\u{1F919}','\u{1F448}','\u{1F449}','\u{1F446}','\u{1F595}','\u{1F447}','\u261D\uFE0F','\u{1F44D}',
            '\u{1F44E}','\u270A','\u{1F44A}','\u{1F91B}','\u{1F91C}','\u{1F44F}','\u{1F64C}','\u{1F450}','\u{1F932}','\u{1F91D}',
            '\u{1F64F}'
        ]
    },
    {
        icon: '\u2764\uFE0F', name: 'Символы',
        emojis: [
            '\u2764\uFE0F','\u{1F9E1}','\u{1F49B}','\u{1F49A}','\u{1F499}','\u{1F49C}','\u{1F5A4}','\u{1F90D}','\u{1F90E}','\u{1F494}',
            '\u2763\uFE0F','\u{1F495}','\u{1F49E}','\u{1F493}','\u{1F497}','\u{1F496}','\u{1F498}','\u{1F49D}','\u{1F49F}','\u2705',
            '\u2611\uFE0F','\u2714\uFE0F','\u274E','\u2795','\u2796','\u2797','\u27B0','\u27BF','\u3030\uFE0F','\u{1F4AF}'
        ]
    }
];

// Инициализация группового чата
function initCompanyChat(chatId, userId, pollingMs) {
    _ccChatId = chatId;
    _ccUserId = userId;

    var interval = pollingMs || 2000;

    var container = document.getElementById('chatMessages');
    if (container) {
        container.addEventListener('scroll', function () {
            _ccIsAtBottom = container.scrollHeight - container.scrollTop - container.clientHeight < 40;
        });
    }

    ccLoadMessages();
    ccSetupChatForm();
    ccSetupEmojiPicker();
    ccSetupTextareaAutoResize();
    ccSetupFileAttach();
    ccSetupReplyCancel();

    // Обновляем сообщения с настраиваемым интервалом
    setInterval(ccLoadMessages, interval);
}

function ccSetupChatForm() {
    var form = document.getElementById('chatForm');
    if (form) {
        form.addEventListener('submit', function (e) {
            e.preventDefault();
            ccSendMessage();
        });
    }

    var input = document.getElementById('chatInput');
    if (input) {
        input.addEventListener('keydown', function (e) {
            if (e.key === 'Enter' && !e.shiftKey) {
                e.preventDefault();
                ccSendMessage();
            }
        });
    }
}

function ccSetupTextareaAutoResize() {
    var textarea = document.getElementById('chatInput');
    if (!textarea) return;

    textarea.addEventListener('input', function () {
        this.style.height = 'auto';
        this.style.height = Math.min(this.scrollHeight, 120) + 'px';
    });
}

function ccSetupReplyCancel() {
    var cancelBtn = document.getElementById('cancelReply');
    if (cancelBtn) {
        cancelBtn.addEventListener('click', ccCancelReply);
    }
}

function setReplyTo(messageId, senderName, text) {
    _ccReplyToMessage = { id: messageId, senderName: senderName, text: text };

    var preview = document.getElementById('chatReplyPreview');
    var senderEl = document.getElementById('replyPreviewSender');
    var textEl = document.getElementById('replyPreviewText');

    if (preview && senderEl && textEl) {
        senderEl.textContent = senderName;
        var shortText = text.length > 80 ? text.substring(0, 80) + '\u2026' : text;
        textEl.textContent = shortText || 'Вложение';
        preview.style.display = 'flex';
    }

    var input = document.getElementById('chatInput');
    if (input) input.focus();
}

function ccCancelReply() {
    _ccReplyToMessage = null;
    var preview = document.getElementById('chatReplyPreview');
    if (preview) preview.style.display = 'none';
}

// === Панель эмодзи ===
function ccSetupEmojiPicker() {
    var toggle = document.getElementById('emojiToggle');
    var picker = document.getElementById('emojiPicker');
    if (!toggle || !picker) return;

    toggle.addEventListener('click', function () {
        if (!_ccEmojiPickerReady) {
            ccBuildEmojiPicker();
            _ccEmojiPickerReady = true;
        }
        picker.classList.toggle('open');
        var icon = toggle.querySelector('i');
        if (picker.classList.contains('open')) {
            icon.className = 'bi bi-keyboard';
        } else {
            icon.className = 'bi bi-emoji-smile';
        }
    });
}

function ccBuildEmojiPicker() {
    var picker = document.getElementById('emojiPicker');

    var tabsHtml = '<div class="emoji-picker-tabs">';
    CC_EMOJI_CATEGORIES.forEach(function (cat, i) {
        tabsHtml += '<button type="button" data-cat="' + i + '"' +
            (i === 0 ? ' class="active"' : '') + ' title="' + ccEscapeHtml(cat.name) + '">' +
            cat.icon + '</button>';
    });
    tabsHtml += '</div>';

    var gridHtml = '<div class="emoji-picker-grid" id="emojiGrid"></div>';
    picker.innerHTML = tabsHtml + gridHtml;

    ccRenderEmojiCategory(0);

    picker.querySelectorAll('.emoji-picker-tabs button').forEach(function (btn) {
        btn.addEventListener('click', function () {
            picker.querySelectorAll('.emoji-picker-tabs button').forEach(function (b) {
                b.classList.remove('active');
            });
            btn.classList.add('active');
            ccRenderEmojiCategory(parseInt(btn.dataset.cat));
        });
    });
}

function ccRenderEmojiCategory(catIndex) {
    var grid = document.getElementById('emojiGrid');
    if (!grid) return;

    var emojis = CC_EMOJI_CATEGORIES[catIndex].emojis;
    grid.innerHTML = emojis.map(function (emoji) {
        return '<button type="button" data-emoji="' + emoji + '">' + emoji + '</button>';
    }).join('');

    grid.querySelectorAll('button').forEach(function (btn) {
        btn.addEventListener('click', function () {
            ccInsertEmoji(btn.dataset.emoji);
        });
    });
}

function ccInsertEmoji(emoji) {
    var textarea = document.getElementById('chatInput');
    if (!textarea) return;

    var start = textarea.selectionStart;
    var end = textarea.selectionEnd;
    var text = textarea.value;
    textarea.value = text.substring(0, start) + emoji + text.substring(end);
    textarea.selectionStart = textarea.selectionEnd = start + emoji.length;
    textarea.focus();
    textarea.dispatchEvent(new Event('input'));
}

// === Прикрепление файлов ===
function ccSetupFileAttach() {
    var attachBtn = document.getElementById('chatAttachBtn');
    var fileInput = document.getElementById('chatFileInput');
    if (!attachBtn || !fileInput) return;

    attachBtn.addEventListener('click', function () {
        fileInput.click();
    });

    fileInput.addEventListener('change', function () {
        for (var i = 0; i < fileInput.files.length; i++) {
            _ccSelectedFiles.push(fileInput.files[i]);
        }
        fileInput.value = '';
        ccRenderFilePreview();
    });
}

function ccRenderFilePreview() {
    var container = document.getElementById('chatFilePreview');
    if (!container) return;

    if (_ccSelectedFiles.length === 0) {
        container.innerHTML = '';
        container.style.display = 'none';
        return;
    }

    container.style.display = 'flex';
    var html = '';
    _ccSelectedFiles.forEach(function (file, index) {
        var isImage = file.type.startsWith('image/');
        var isVideo = file.type.startsWith('video/');
        var thumbHtml = '';

        if (isImage) {
            thumbHtml = '<img src="' + URL.createObjectURL(file) + '" alt="" class="chat-file-thumb" />';
        } else if (isVideo) {
            thumbHtml = '<div class="chat-file-thumb-icon"><i class="bi bi-camera-video-fill"></i></div>';
        } else {
            thumbHtml = '<div class="chat-file-thumb-icon"><i class="bi bi-file-earmark"></i></div>';
        }

        html += '<div class="chat-file-item">' +
            thumbHtml +
            '<button type="button" class="chat-file-remove" onclick="removeSelectedFile(' + index + ')" title="Удалить">&times;</button>' +
            '<div class="chat-file-name">' + ccEscapeHtml(file.name) + '</div>' +
            '</div>';
    });

    container.innerHTML = html;
}

function removeSelectedFile(index) {
    _ccSelectedFiles.splice(index, 1);
    ccRenderFilePreview();
}

function ccGetAvatarColor(senderId) {
    return CC_AVATAR_COLORS[senderId % CC_AVATAR_COLORS.length];
}

// === Загрузка и отрисовка сообщений ===
async function ccLoadMessages() {
    if (!_ccChatId) return;

    try {
        var response = await fetch('/api/company-chat/' + _ccChatId + '/messages');
        if (response.ok) {
            var messages = await response.json();
            if (messages.length !== _ccLastMessageCount) {
                _ccLastMessageCount = messages.length;
                ccRenderMessages(messages);
            }
            // Отмечаем сообщения как прочитанные
            ccMarkAsRead();
        }
    } catch (e) { }
}

// Отметить все сообщения в чате как прочитанные
async function ccMarkAsRead() {
    if (!_ccChatId) return;
    try {
        await fetch('/api/company-chat/' + _ccChatId + '/read', { method: 'POST' });
    } catch (e) { }
}

function ccRenderMessages(messages) {
    var container = document.getElementById('chatMessages');
    if (!container) return;

    _ccAttachmentGroups = [];

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

    messages.forEach(function (msg) {
        var msgDate = new Date(msg.createdAt);
        var dateStr = ccFormatDate(msgDate);

        if (dateStr !== lastDate) {
            html += '<div class="chat-date-separator">' + dateStr + '</div>';
            lastDate = dateStr;
        }

        var isOwn = msg.senderId === _ccUserId;
        var time = msgDate.toLocaleTimeString('ru-RU', { hour: '2-digit', minute: '2-digit' });

        // Вложения
        var attachmentsHtml = '';
        if (msg.attachments && msg.attachments.length > 0) {
            var msgIndex = _ccAttachmentGroups.length;
            _ccAttachmentGroups.push(msg.attachments);

            attachmentsHtml = '<div class="chat-attachments d-flex flex-wrap gap-2 mt-1">' +
                msg.attachments.map(function (att, attIndex) {
                    var isImage = att.fileName.match(/\.(jpg|jpeg|png|gif|webp|bmp)$/i);
                    var isVideo = att.fileName.match(/\.(mp4|webm|mov|avi)$/i);

                    if (isImage) {
                        return '<img src="' + ccEscapeHtml(att.filePath) + '" alt="' + ccEscapeHtml(att.fileName) + '" ' +
                            'class="chat-attachment-img" style="max-width:200px;max-height:150px;object-fit:cover;" ' +
                            'onclick="openGallery(' + msgIndex + ',' + attIndex + ')" />';
                    } else if (isVideo) {
                        return '<div class="position-relative d-inline-block" style="cursor:pointer;" ' +
                            'onclick="openGallery(' + msgIndex + ',' + attIndex + ')">' +
                            '<video src="' + ccEscapeHtml(att.filePath) + '" ' +
                            'class="rounded" style="max-width:250px;max-height:180px;pointer-events:none;"></video>' +
                            '<div class="position-absolute top-50 start-50 translate-middle text-white bg-dark bg-opacity-50 rounded-circle d-flex align-items-center justify-content-center" style="width:40px;height:40px;">' +
                            '<i class="bi bi-play-fill fs-5"></i></div></div>';
                    } else {
                        return '<a href="javascript:void(0)" class="btn btn-sm btn-outline-secondary" ' +
                            'onclick="openGallery(' + msgIndex + ',' + attIndex + ')">' +
                            '<i class="bi bi-paperclip"></i> ' + ccEscapeHtml(att.fileName) +
                            '</a>';
                    }
                }).join('') +
                '</div>';
        }

        // Галочки прочтения
        var readCheck = '';
        if (isOwn) {
            readCheck = msg.isRead
                ? '<span class="read-check read" title="Прочитано"><i class="bi bi-check2-all"></i></span>'
                : '<span class="read-check" title="Доставлено"><i class="bi bi-check2"></i></span>';
        }

        var textHtml = ccLinkify(ccEscapeHtml(msg.text || ''));

        // Блок ответа
        var replyHtml = '';
        if (msg.replyTo) {
            var replyText = ccEscapeHtml(msg.replyTo.text || 'Вложение');
            replyHtml = '<div class="chat-reply-block" onclick="scrollToMessage(' + msg.replyTo.id + ')">' +
                '<div class="chat-reply-sender">' + ccEscapeHtml(msg.replyTo.senderName) + '</div>' +
                '<div class="chat-reply-text">' + replyText + '</div>' +
                '</div>';
        }

        // Аватарка
        var avatarHtml = '';
        if (!isOwn) {
            var avatarColor = ccGetAvatarColor(msg.senderId);
            if (msg.senderAvatarUrl) {
                avatarHtml = '<img src="' + ccEscapeHtml(msg.senderAvatarUrl) + '" class="chat-avatar" alt="" />';
            } else {
                avatarHtml = '<div class="chat-avatar chat-avatar-initials" style="background:' + avatarColor + '">' +
                    ccEscapeHtml(msg.senderInitials || '?') + '</div>';
            }
        }

        // Кнопка ответа
        var replyBtnHtml = '<button class="chat-reply-btn" onclick="setReplyTo(' + msg.id + ', \'' +
            ccEscapeHtml(msg.senderName).replace(/'/g, "\\'") + '\', \'' +
            ccEscapeHtml(msg.text || '').replace(/'/g, "\\'").replace(/\n/g, ' ') + '\')" title="Ответить">' +
            '<i class="bi bi-reply-fill"></i></button>';

        html +=
            '<div class="chat-message-row ' + (isOwn ? 'own' : 'incoming') + '" id="msg-' + msg.id + '">' +
            (isOwn ? '' : '<div class="chat-avatar-col">' + avatarHtml + '</div>') +
            '<div class="chat-message ' + (isOwn ? 'own' : 'incoming') + '">' +
            (isOwn ? '' : '<div class="sender">' + ccEscapeHtml(msg.senderName) + '</div>') +
            replyHtml +
            '<div class="msg-text">' + textHtml +
            '<span class="msg-meta">' + time + readCheck + '</span>' +
            '</div>' +
            attachmentsHtml +
            '</div>' +
            replyBtnHtml +
            '</div>';
    });

    container.innerHTML = html;

    if (_ccIsAtBottom) {
        container.scrollTop = container.scrollHeight;
    }
}

function scrollToMessage(messageId) {
    var el = document.getElementById('msg-' + messageId);
    if (!el) return;

    el.scrollIntoView({ behavior: 'smooth', block: 'center' });
    el.classList.add('chat-message-highlight');
    setTimeout(function () {
        el.classList.remove('chat-message-highlight');
    }, 1500);
}

function ccFormatDate(date) {
    var today = new Date();
    var yesterday = new Date();
    yesterday.setDate(yesterday.getDate() - 1);

    if (date.toDateString() === today.toDateString()) return 'Сегодня';
    if (date.toDateString() === yesterday.toDateString()) return 'Вчера';

    return date.toLocaleDateString('ru-RU', { day: 'numeric', month: 'long', year: 'numeric' });
}

function ccLinkify(text) {
    var urlRegex = /(https?:\/\/[^\s<]+)/g;
    return text.replace(urlRegex, function (url) {
        return '<a href="' + url + '" target="_blank" rel="noopener noreferrer">' + url + '</a>';
    });
}

// === Отправка сообщения ===
async function ccSendMessage() {
    var input = document.getElementById('chatInput');
    var text = input.value.trim();
    var hasFiles = _ccSelectedFiles.length > 0;

    if (!text && !hasFiles) return;
    if (!_ccChatId) return;

    // Закрываем эмодзи-панель
    var picker = document.getElementById('emojiPicker');
    if (picker) picker.classList.remove('open');
    var emojiIcon = document.querySelector('#emojiToggle i');
    if (emojiIcon) emojiIcon.className = 'bi bi-emoji-smile';

    var savedText = text;
    var savedFiles = _ccSelectedFiles.slice();
    var savedReply = _ccReplyToMessage;

    input.value = '';
    input.style.height = 'auto';
    _ccSelectedFiles = [];
    ccRenderFilePreview();
    ccCancelReply();

    try {
        var response;
        var replyId = savedReply ? savedReply.id : null;

        if (hasFiles) {
            var formData = new FormData();
            formData.append('text', text);
            if (replyId) formData.append('replyToMessageId', replyId);
            savedFiles.forEach(function (file) {
                formData.append('files', file);
            });

            response = await fetch('/api/company-chat/' + _ccChatId + '/send-with-files', {
                method: 'POST',
                body: formData
            });
        } else {
            var body = { text: text };
            if (replyId) body.replyToMessageId = replyId;

            response = await fetch('/api/company-chat/' + _ccChatId + '/send', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(body)
            });
        }

        if (response.ok) {
            _ccLastMessageCount = 0;
            _ccIsAtBottom = true;
            ccLoadMessages();
        }
    } catch (e) {
        input.value = savedText;
        input.dispatchEvent(new Event('input'));
        _ccSelectedFiles = savedFiles;
        ccRenderFilePreview();
        if (savedReply) {
            setReplyTo(savedReply.id, savedReply.senderName, savedReply.text);
        }
    }
}

// === Галерея вложений ===
function openGallery(groupIndex, itemIndex) {
    _ccGalleryItems = _ccAttachmentGroups[groupIndex] || [];
    _ccGalleryIndex = itemIndex;
    ccRenderGallerySlide();

    var modalEl = document.getElementById('attachmentModal');
    var modal = bootstrap.Modal.getInstance(modalEl) || new bootstrap.Modal(modalEl);
    modal.show();

    modalEl.addEventListener('hidden.bs.modal', function () {
        var video = document.getElementById('galleryContent').querySelector('video');
        if (video) video.pause();
        document.removeEventListener('keydown', _ccGalleryKeyHandler);
    }, { once: true });

    document.removeEventListener('keydown', _ccGalleryKeyHandler);
    document.addEventListener('keydown', _ccGalleryKeyHandler);
}

function _ccGalleryKeyHandler(e) {
    if (e.key === 'ArrowLeft') galleryNav(-1);
    if (e.key === 'ArrowRight') galleryNav(1);
}

function galleryNav(direction) {
    var video = document.getElementById('galleryContent').querySelector('video');
    if (video) video.pause();

    _ccGalleryIndex += direction;
    if (_ccGalleryIndex < 0) _ccGalleryIndex = _ccGalleryItems.length - 1;
    if (_ccGalleryIndex >= _ccGalleryItems.length) _ccGalleryIndex = 0;
    ccRenderGallerySlide();
}

function ccRenderGallerySlide() {
    var att = _ccGalleryItems[_ccGalleryIndex];
    var content = document.getElementById('galleryContent');
    var title = document.getElementById('attachmentModalTitle');
    var counter = document.getElementById('attachmentCounter');
    var prevBtn = document.getElementById('galleryPrev');
    var nextBtn = document.getElementById('galleryNext');

    title.textContent = att.fileName;
    counter.textContent = (_ccGalleryIndex + 1) + ' / ' + _ccGalleryItems.length;

    var showNav = _ccGalleryItems.length > 1;
    prevBtn.style.display = showNav ? '' : 'none';
    nextBtn.style.display = showNav ? '' : 'none';
    counter.style.display = showNav ? '' : 'none';

    var isImage = att.fileName.match(/\.(jpg|jpeg|png|gif|webp|bmp)$/i);
    var isVideo = att.fileName.match(/\.(mp4|webm|mov|avi)$/i);

    if (isImage) {
        content.innerHTML = '<img src="' + ccEscapeHtml(att.filePath) + '" alt="' + ccEscapeHtml(att.fileName) + '" ' +
            'class="img-fluid rounded" style="max-height:75vh;" />';
    } else if (isVideo) {
        content.innerHTML = '<video src="' + ccEscapeHtml(att.filePath) + '" controls autoplay ' +
            'class="rounded" style="max-width:100%;max-height:75vh;"></video>';
    } else {
        content.innerHTML = '<div class="py-4">' +
            '<i class="bi bi-file-earmark fs-1 text-muted"></i>' +
            '<p class="mt-2">' + ccEscapeHtml(att.fileName) + '</p>' +
            '<a href="' + ccEscapeHtml(att.filePath) + '" download class="btn btn-primary">' +
            '<i class="bi bi-download"></i> Скачать</a></div>';
    }
}

function ccEscapeHtml(text) {
    if (!text) return '';
    var div = document.createElement('div');
    div.textContent = text;
    return div.innerHTML;
}
