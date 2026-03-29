// Логика страницы списка чатов

let _chatsUserId = null;
let _chatsEmployees = [];

const CHATS_AVATAR_COLORS = [
    '#e17076', '#7bc862', '#e5ca77', '#65aadd',
    '#a695e7', '#ee7aae', '#6ec9cb', '#faa774'
];

const ROLE_NAMES = {
    0: 'Техник',
    1: 'Инженер',
    2: 'Главный инженер',
    3: 'Логист',
    4: 'Менеджер TW',
    5: 'Клиент',
    6: 'Менеджер клиента',
    7: 'Модератор'
};

function initChats(userId) {
    _chatsUserId = userId;
    loadChatList();
    setupNewChatModal();

    // Обновляем список чатов каждые 10 секунд
    setInterval(loadChatList, 10000);
}

// === Загрузка списка чатов ===
async function loadChatList() {
    try {
        var response = await fetch('/api/chats');
        if (!response.ok) return;

        var chats = await response.json();
        renderChatList(chats);
    } catch (e) { }
}

function renderChatList(chats) {
    var container = document.getElementById('chatList');
    if (!container) return;

    if (chats.length === 0) {
        container.innerHTML =
            '<div class="text-center text-muted py-5">' +
            '<i class="bi bi-chat-square-text fs-1 d-block mb-2"></i>' +
            '<p>У вас пока нет чатов</p>' +
            '<button class="btn btn-primary btn-sm" onclick="openNewChatModal()">' +
            '<i class="bi bi-plus-lg"></i> Начать чат</button>' +
            '</div>';
        return;
    }

    var html = '';
    chats.forEach(function (chat) {
        var avatarHtml = '';
        if (chat.isDirectChat) {
            var color = CHATS_AVATAR_COLORS[(chat.chatId) % CHATS_AVATAR_COLORS.length];
            if (chat.avatarUrl) {
                avatarHtml = '<img src="' + chatsEscape(chat.avatarUrl) + '" class="chat-list-avatar" alt="" />';
            } else {
                avatarHtml = '<div class="chat-list-avatar chat-list-avatar-initials" style="background:' + color + '">' +
                    chatsEscape(chat.initials || '?') + '</div>';
            }
        } else {
            avatarHtml = '<div class="chat-list-avatar chat-list-avatar-initials" style="background:var(--accent)">' +
                '<i class="bi bi-people-fill" style="font-size:1rem"></i></div>';
        }

        var lastMsgHtml = '';
        if (chat.lastMessage) {
            var sender = chat.lastMessageSender ? chatsEscape(chat.lastMessageSender).split(' ')[0] + ': ' : '';
            var msgText = chatsEscape(chat.lastMessage);
            if (msgText.length > 50) msgText = msgText.substring(0, 50) + '\u2026';
            lastMsgHtml = '<div class="chat-list-last-msg">' + sender + msgText + '</div>';
        } else {
            lastMsgHtml = '<div class="chat-list-last-msg text-muted">Нет сообщений</div>';
        }

        var timeHtml = '';
        if (chat.lastMessageAt) {
            timeHtml = '<div class="chat-list-time">' + formatChatTime(chat.lastMessageAt) + '</div>';
        }

        var badgeHtml = '';
        if (chat.unreadCount > 0) {
            badgeHtml = '<div class="chat-list-badge">' + chat.unreadCount + '</div>';
        }

        var chatUrl = chat.isDirectChat
            ? '/Chats/Chat/' + chat.chatId
            : '/CompanyChat/Index';

        html +=
            '<a href="' + chatUrl + '" class="chat-list-item">' +
            '<div class="chat-list-avatar-col">' + avatarHtml + '</div>' +
            '<div class="chat-list-content">' +
            '<div class="chat-list-header">' +
            '<div class="chat-list-name">' + chatsEscape(chat.name) + '</div>' +
            timeHtml +
            '</div>' +
            '<div class="chat-list-footer">' +
            lastMsgHtml +
            badgeHtml +
            '</div>' +
            '</div>' +
            '</a>';
    });

    container.innerHTML = html;
}

function formatChatTime(dateStr) {
    var date = new Date(dateStr);
    var now = new Date();
    var today = new Date(now.getFullYear(), now.getMonth(), now.getDate());
    var msgDay = new Date(date.getFullYear(), date.getMonth(), date.getDate());

    if (msgDay.getTime() === today.getTime()) {
        return date.toLocaleTimeString('ru-RU', { hour: '2-digit', minute: '2-digit' });
    }

    var yesterday = new Date(today);
    yesterday.setDate(yesterday.getDate() - 1);
    if (msgDay.getTime() === yesterday.getTime()) {
        return 'Вчера';
    }

    return date.toLocaleDateString('ru-RU', { day: '2-digit', month: '2-digit' });
}

// === Модальное окно нового чата ===
function setupNewChatModal() {
    var btn = document.getElementById('newChatBtn');
    if (btn) {
        btn.addEventListener('click', openNewChatModal);
    }

    var searchInput = document.getElementById('employeeSearch');
    if (searchInput) {
        searchInput.addEventListener('input', function () {
            filterEmployees(this.value);
        });
    }
}

function openNewChatModal() {
    loadEmployees();
    var modalEl = document.getElementById('newChatModal');
    var modal = bootstrap.Modal.getInstance(modalEl) || new bootstrap.Modal(modalEl);
    modal.show();

    // Фокус на поле поиска
    modalEl.addEventListener('shown.bs.modal', function () {
        var searchInput = document.getElementById('employeeSearch');
        if (searchInput) {
            searchInput.value = '';
            searchInput.focus();
        }
    }, { once: true });
}

async function loadEmployees() {
    var container = document.getElementById('employeeList');
    if (!container) return;

    container.innerHTML = '<div class="text-center text-muted py-3"><div class="spinner-border spinner-border-sm"></div> Загрузка...</div>';

    try {
        var response = await fetch('/api/chats/employees');
        if (!response.ok) return;

        _chatsEmployees = await response.json();
        renderEmployees(_chatsEmployees);
    } catch (e) {
        container.innerHTML = '<div class="text-center text-muted py-3">Ошибка загрузки</div>';
    }
}

function renderEmployees(employees) {
    var container = document.getElementById('employeeList');
    if (!container) return;

    if (employees.length === 0) {
        container.innerHTML = '<div class="text-center text-muted py-3">Сотрудники не найдены</div>';
        return;
    }

    var html = '';
    employees.forEach(function (emp) {
        var color = CHATS_AVATAR_COLORS[emp.userId % CHATS_AVATAR_COLORS.length];
        var avatarHtml;
        if (emp.avatarUrl) {
            avatarHtml = '<img src="' + chatsEscape(emp.avatarUrl) + '" class="chat-list-avatar-sm" alt="" />';
        } else {
            var nameParts = emp.fullName.split(' ');
            var initials = '';
            if (nameParts.length >= 2) {
                initials = (nameParts[0][0] + nameParts[1][0]).toUpperCase();
            } else if (nameParts.length === 1) {
                initials = nameParts[0][0].toUpperCase();
            }
            avatarHtml = '<div class="chat-list-avatar-sm chat-list-avatar-initials" style="background:' + color + '">' +
                chatsEscape(initials) + '</div>';
        }

        var roleName = ROLE_NAMES[emp.role] || '';

        html +=
            '<div class="employee-item" onclick="startDirectChat(' + emp.userId + ')">' +
            avatarHtml +
            '<div class="employee-info">' +
            '<div class="employee-name">' + chatsEscape(emp.fullName) + '</div>' +
            '<div class="employee-role">' + chatsEscape(roleName) + '</div>' +
            '</div>' +
            '</div>';
    });

    container.innerHTML = html;
}

function filterEmployees(query) {
    if (!query) {
        renderEmployees(_chatsEmployees);
        return;
    }

    var q = query.toLowerCase();
    var filtered = _chatsEmployees.filter(function (emp) {
        return emp.fullName.toLowerCase().indexOf(q) !== -1;
    });
    renderEmployees(filtered);
}

async function startDirectChat(targetUserId) {
    try {
        var response = await fetch('/api/chats/direct/' + targetUserId, { method: 'POST' });
        if (!response.ok) return;

        var data = await response.json();

        // Закрываем модальное окно
        var modalEl = document.getElementById('newChatModal');
        var modal = bootstrap.Modal.getInstance(modalEl);
        if (modal) modal.hide();

        // Переходим в чат
        window.location.href = '/Chats/Chat/' + data.chatId;
    } catch (e) { }
}

function chatsEscape(text) {
    if (!text) return '';
    var div = document.createElement('div');
    div.textContent = text;
    return div.innerHTML;
}
