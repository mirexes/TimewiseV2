// Логика работы с заявками (AJAX)

// Смена статуса заявки (без фото)
async function updateStatus(ticketId, newStatus) {
    const comment = prompt('Комментарий (необязательно):');

    const formData = new FormData();
    formData.append('ticketId', ticketId);
    formData.append('newStatus', newStatus);
    if (comment) formData.append('comment', comment);

    try {
        const response = await fetch('/api/tickets/status', {
            method: 'POST',
            body: formData
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

// Открытие модального окна завершения с фото
function showCompletionModal(ticketId, newStatus) {
    document.getElementById('completionTicketId').value = ticketId;
    document.getElementById('completionNewStatus').value = newStatus;
    document.getElementById('completionComment').value = '';
    document.getElementById('completionPhotos').value = '';
    document.getElementById('completionPhotoPreview').innerHTML = '';
    new bootstrap.Modal(document.getElementById('completionModal')).show();
}

// Превью выбранных фото
document.addEventListener('DOMContentLoaded', function () {
    const photosInput = document.getElementById('completionPhotos');
    if (photosInput) {
        photosInput.addEventListener('change', function () {
            const preview = document.getElementById('completionPhotoPreview');
            preview.innerHTML = '';
            Array.from(this.files).forEach(file => {
                if (!file.type.startsWith('image/')) return;
                const col = document.createElement('div');
                col.className = 'col-4';
                const img = document.createElement('img');
                img.className = 'img-fluid rounded';
                img.style.cssText = 'height:80px;width:100%;object-fit:cover;';
                img.src = URL.createObjectURL(file);
                col.appendChild(img);
                preview.appendChild(col);
            });
        });
    }
});

// Отправка завершения с фото
async function submitCompletion() {
    const ticketId = document.getElementById('completionTicketId').value;
    const newStatus = document.getElementById('completionNewStatus').value;
    const comment = document.getElementById('completionComment').value;
    const photosInput = document.getElementById('completionPhotos');

    const formData = new FormData();
    formData.append('ticketId', ticketId);
    formData.append('newStatus', newStatus);
    if (comment) formData.append('comment', comment);

    if (photosInput.files.length > 0) {
        Array.from(photosInput.files).forEach(file => {
            formData.append('photos', file);
        });
    }

    try {
        const response = await fetch('/api/tickets/status', {
            method: 'POST',
            body: formData
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

// Галерея фото акта выполненных работ
var completionGalleryIndex = 0;

function openCompletionGallery(index) {
    if (!completionPhotos || completionPhotos.length === 0) return;
    completionGalleryIndex = index;
    renderCompletionGallery();
    new bootstrap.Modal(document.getElementById('completionGalleryModal')).show();
}

function renderCompletionGallery() {
    const photo = completionPhotos[completionGalleryIndex];
    const content = document.getElementById('completionGalleryContent');
    content.innerHTML = `<img src="${photo.filePath}" alt="${photo.fileName}" class="img-fluid" style="max-height:70vh;" />`;

    const counter = document.getElementById('completionGalleryCounter');
    counter.textContent = `${completionGalleryIndex + 1} / ${completionPhotos.length}`;

    document.getElementById('completionGalleryPrev').style.display = completionGalleryIndex > 0 ? '' : 'none';
    document.getElementById('completionGalleryNext').style.display = completionGalleryIndex < completionPhotos.length - 1 ? '' : 'none';
}

function completionGalleryNav(direction) {
    completionGalleryIndex += direction;
    if (completionGalleryIndex < 0) completionGalleryIndex = 0;
    if (completionGalleryIndex >= completionPhotos.length) completionGalleryIndex = completionPhotos.length - 1;
    renderCompletionGallery();
}

// Загрузка списка специалистов в выпадающий список
async function loadEngineers() {
    const select = document.getElementById('engineerSelect');
    if (!select) return;

    try {
        const response = await fetch('/api/tickets/engineers');
        if (!response.ok) return;

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
