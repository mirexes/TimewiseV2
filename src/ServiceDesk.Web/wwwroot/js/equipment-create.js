'use strict';

// === Превью фото оборудования ===
(function () {
    var input = document.getElementById('photoInput');
    var preview = document.getElementById('photoPreview');
    if (!input || !preview) return;

    input.addEventListener('change', function () {
        preview.innerHTML = '';
        var file = input.files[0];
        if (!file) return;

        var wrapper = document.createElement('div');
        wrapper.style.cssText = 'width:120px;height:120px;border-radius:8px;overflow:hidden;border:1px solid var(--bs-border-color);';

        var img = document.createElement('img');
        img.style.cssText = 'width:100%;height:100%;object-fit:cover;';
        img.src = URL.createObjectURL(file);
        img.onload = function () { URL.revokeObjectURL(img.src); };
        wrapper.appendChild(img);

        preview.appendChild(wrapper);
    });
})();

// === Поиск по точкам обслуживания с фильтрацией ===
(function () {
    var searchInput = document.getElementById('servicePointSearch');
    var hiddenInput = document.getElementById('servicePointValue');
    var listEl = document.getElementById('servicePointList');
    var dataScript = document.getElementById('spOptionsData');
    if (!searchInput || !hiddenInput || !listEl || !dataScript) return;

    var allOptions = [];
    try { allOptions = JSON.parse(dataScript.textContent); } catch (e) { return; }

    // Если уже есть выбранное значение — показать текст
    if (hiddenInput.value) {
        var sel = allOptions.find(function (o) { return o.Id == hiddenInput.value; });
        if (sel) searchInput.value = sel.Text;
    }

    var activeIndex = -1;

    function clearSelection() {
        hiddenInput.value = '';
    }

    function selectOption(opt) {
        searchInput.value = opt.Text;
        hiddenInput.value = opt.Id;
        hideList();
    }

    function renderList(items) {
        listEl.innerHTML = '';
        activeIndex = -1;
        if (items.length === 0) {
            var empty = document.createElement('div');
            empty.className = 'list-group-item text-muted small py-2';
            empty.textContent = 'Ничего не найдено';
            listEl.appendChild(empty);
            listEl.style.display = 'block';
            return;
        }
        items.forEach(function (opt) {
            var btn = document.createElement('button');
            btn.type = 'button';
            btn.className = 'list-group-item list-group-item-action small py-2';
            btn.textContent = opt.Text;
            btn.addEventListener('mousedown', function (e) {
                e.preventDefault();
                selectOption(opt);
            });
            listEl.appendChild(btn);
        });
        listEl.style.display = 'block';
    }

    function hideList() {
        listEl.style.display = 'none';
        listEl.innerHTML = '';
        activeIndex = -1;
    }

    function filterAndShow() {
        var query = searchInput.value.trim().toLowerCase();
        var filtered;
        if (query.length === 0) {
            filtered = allOptions.slice(0, 50);
        } else {
            filtered = allOptions.filter(function (o) {
                return o.Text.toLowerCase().indexOf(query) !== -1;
            }).slice(0, 50);
        }
        renderList(filtered);
    }

    searchInput.addEventListener('focus', function () {
        filterAndShow();
    });

    searchInput.addEventListener('input', function () {
        clearSelection();
        filterAndShow();
    });

    searchInput.addEventListener('keydown', function (e) {
        var items = listEl.querySelectorAll('.list-group-item-action');
        if (!items.length) return;

        if (e.key === 'ArrowDown') {
            e.preventDefault();
            activeIndex = Math.min(activeIndex + 1, items.length - 1);
            updateActive(items);
        } else if (e.key === 'ArrowUp') {
            e.preventDefault();
            activeIndex = Math.max(activeIndex - 1, 0);
            updateActive(items);
        } else if (e.key === 'Enter') {
            e.preventDefault();
            if (activeIndex >= 0 && activeIndex < items.length) {
                items[activeIndex].dispatchEvent(new MouseEvent('mousedown'));
            }
        } else if (e.key === 'Escape') {
            hideList();
        }
    });

    function updateActive(items) {
        items.forEach(function (el, i) {
            el.classList.toggle('active', i === activeIndex);
            if (i === activeIndex) el.scrollIntoView({ block: 'nearest' });
        });
    }

    // Скрываем список при клике вне
    document.addEventListener('mousedown', function (e) {
        if (!searchInput.contains(e.target) && !listEl.contains(e.target)) {
            hideList();
        }
    });
})();
