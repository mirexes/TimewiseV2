'use strict';

// === Превью вложений (фото/видео) ===
(function () {
    const input = document.getElementById('attachmentsInput');
    const preview = document.getElementById('attachmentPreview');
    if (!input || !preview) return;

    input.addEventListener('change', function () {
        preview.innerHTML = '';
        const files = input.files;
        if (!files || files.length === 0) return;

        for (let i = 0; i < files.length; i++) {
            const file = files[i];
            const wrapper = document.createElement('div');
            wrapper.className = 'position-relative';
            wrapper.style.cssText = 'width:80px;height:80px;border-radius:8px;overflow:hidden;border:1px solid var(--bs-border-color);';

            if (file.type.startsWith('image/')) {
                const img = document.createElement('img');
                img.style.cssText = 'width:100%;height:100%;object-fit:cover;';
                img.src = URL.createObjectURL(file);
                img.onload = function () { URL.revokeObjectURL(img.src); };
                wrapper.appendChild(img);
            } else if (file.type.startsWith('video/')) {
                const icon = document.createElement('div');
                icon.className = 'd-flex align-items-center justify-content-center h-100 bg-dark text-white';
                icon.innerHTML = '<i class="bi bi-camera-video fs-4"></i>';
                wrapper.appendChild(icon);
            }

            const label = document.createElement('div');
            label.className = 'text-truncate small text-center';
            label.style.cssText = 'width:80px;font-size:0.65rem;';
            label.textContent = file.name;

            const col = document.createElement('div');
            col.appendChild(wrapper);
            col.appendChild(label);
            preview.appendChild(col);
        }
    });
})();

// === Поиск по клиентам (торговая сеть) с валидацией ===
(function () {
    var searchInput = document.getElementById('clientSearch');
    var hiddenInput = document.getElementById('clientValue');
    var listEl = document.getElementById('clientList');
    var dataScript = document.getElementById('clientOptionsData');
    var validationError = document.getElementById('clientValidationError');
    if (!searchInput || !hiddenInput || !listEl || !dataScript) return;

    var clientOptions = [];
    try { clientOptions = JSON.parse(dataScript.textContent); } catch (e) { return; }

    // Если уже есть выбранное значение — показать текст
    if (hiddenInput.value) {
        var sel = clientOptions.find(function (o) { return o.Id == hiddenInput.value; });
        if (sel) searchInput.value = sel.Text;
    }

    var activeIndex = -1;

    function selectClient(opt) {
        searchInput.value = opt.Text;
        hiddenInput.value = opt.Id;
        hideList();
        if (validationError) validationError.style.display = 'none';
        searchInput.classList.remove('is-invalid');
        // Уведомляем об изменении клиента для фильтрации точек обслуживания
        searchInput.dispatchEvent(new CustomEvent('clientChanged', { detail: { clientId: opt.Id } }));
    }

    function clearClient() {
        hiddenInput.value = '';
        searchInput.dispatchEvent(new CustomEvent('clientChanged', { detail: { clientId: '' } }));
    }

    function renderList(items) {
        listEl.innerHTML = '';
        activeIndex = -1;
        if (items.length === 0) {
            var empty = document.createElement('div');
            empty.className = 'list-group-item text-muted small py-2';
            empty.innerHTML = 'Ничего не найдено. <a href="/Clients/Create" target="_blank">Создать нового клиента</a>';
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
                selectClient(opt);
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
            filtered = clientOptions.slice(0, 50);
        } else {
            filtered = clientOptions.filter(function (o) {
                return o.Text.toLowerCase().indexOf(query) !== -1;
            }).slice(0, 50);
        }
        renderList(filtered);
    }

    searchInput.addEventListener('focus', function () {
        filterAndShow();
    });

    searchInput.addEventListener('input', function () {
        clearClient();
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

    // Валидация при отправке формы — клиент обязателен
    var form = searchInput.closest('form');
    if (form) {
        form.addEventListener('submit', function (e) {
            if (!hiddenInput.value) {
                e.preventDefault();
                searchInput.classList.add('is-invalid');
                if (validationError) validationError.style.display = 'block';
                searchInput.focus();
            }
        });
    }
})();

// === Яндекс Карты — выбор нового адреса ===
// Модуль карты регистрирует интерфейс window.ticketAddressMap,
// которым пользуется объединённый поиск точки обслуживания.
(function () {
    var btnShow = document.getElementById('btnShowMap');
    var mapContainer = document.getElementById('mapContainer');

    // Интерфейс по умолчанию (карта недоступна) — поиск работает без подсказок с карты
    window.ticketAddressMap = {
        available: false,
        show: function () { },
        selectAddress: function () { },
        suggest: function () { return Promise.resolve([]); }
    };

    // Карта недоступна, если нет контейнера (нет API-ключа) или не загружен ymaps
    if (!mapContainer) return;
    if (typeof ymaps === 'undefined') {
        console.warn('[ticket-create] ymaps не загружен — проверьте API-ключ Яндекс Карт');
        return;
    }

    var mapInitialized = false;
    var yMap = null;
    var placemark = null;

    function showMap() {
        mapContainer.style.display = 'block';
        if (btnShow) btnShow.innerHTML = '<i class="bi bi-x-lg"></i> Скрыть карту';
        if (!mapInitialized) {
            mapInitialized = true;
            ymaps.ready(initMap);
        }
    }

    function hideMap() {
        mapContainer.style.display = 'none';
        if (btnShow) btnShow.innerHTML = '<i class="bi bi-geo-alt"></i> Указать новый адрес на карте';
    }

    if (btnShow) {
        btnShow.addEventListener('click', function () {
            if (mapContainer.style.display === 'none') showMap();
            else hideMap();
        });
    }

    function initMap() {
        var defaultCenter = [53.346785, 83.776856];
        var defaultZoom = 10;

        yMap = new ymaps.Map('yandexMap', {
            center: defaultCenter,
            zoom: defaultZoom,
            controls: ['zoomControl', 'geolocationControl']
        });

        // Центрируем карту по текущему местоположению пользователя
        if (navigator.geolocation) {
            navigator.geolocation.getCurrentPosition(function (pos) {
                var userCoords = [pos.coords.latitude, pos.coords.longitude];
                yMap.setCenter(userCoords, 14);
            });
        }

        // Клик по карте — установка метки
        yMap.events.add('click', function (e) {
            var coords = e.get('coords');
            setPlacemark(coords);
            reverseGeocode(coords);
        });
    }

    // Получение подсказок адресов (для объединённого поиска точки обслуживания)
    function suggest(query) {
        return new Promise(function (resolve) {
            ymaps.ready(function () {
                if (typeof ymaps.suggest === 'function') {
                    ymaps.suggest(query).then(function (items) {
                        resolve((items || []).map(function (i) {
                            return { displayName: i.displayName, value: i.value };
                        }));
                    }).catch(function () {
                        geocodeSuggest(query).then(resolve);
                    });
                } else {
                    geocodeSuggest(query).then(resolve);
                }
            });
        });
    }

    function geocodeSuggest(query) {
        return ymaps.geocode(query, { results: 5 }).then(function (res) {
            var items = [];
            res.geoObjects.each(function (obj) {
                items.push({ displayName: obj.getAddressLine(), value: obj.getAddressLine() });
            });
            return items;
        }).catch(function () {
            return [];
        });
    }

    function setPlacemark(coords) {
        if (placemark) {
            placemark.geometry.setCoordinates(coords);
        } else {
            placemark = new ymaps.Placemark(coords, {}, {
                draggable: true,
                preset: 'islands#redDotIcon'
            });
            placemark.events.add('dragend', function () {
                reverseGeocode(placemark.geometry.getCoordinates());
            });
            yMap.geoObjects.add(placemark);
        }
    }

    function reverseGeocode(coords) {
        ymaps.geocode(coords).then(function (res) {
            var firstGeoObject = res.geoObjects.get(0);
            if (!firstGeoObject) return;
            showAddress(firstGeoObject.getAddressLine(), coords);
        });
    }

    function geocodeAddress(address) {
        if (!address) return;
        ymaps.geocode(address, { results: 1 }).then(function (res) {
            var firstGeoObject = res.geoObjects.get(0);
            if (!firstGeoObject) return;

            var coords = firstGeoObject.geometry.getCoordinates();
            if (yMap) {
                setPlacemark(coords);
                yMap.setCenter(coords, 16);
            }
            showAddress(firstGeoObject.getAddressLine(), coords);
        });
    }

    function showAddress(address, coords) {
        // Подставляем выбранный адрес в объединённое поле поиска точки обслуживания
        var spSearch = document.getElementById('servicePointSearch');
        if (spSearch) {
            spSearch.value = address;
            spSearch.classList.remove('input-validation-error', 'is-invalid');
        }

        // Заполняем hidden-поля для отправки на сервер
        var hiddenAddress = document.getElementById('newAddress');
        var hiddenLat = document.getElementById('newLatitude');
        var hiddenLng = document.getElementById('newLongitude');
        if (hiddenAddress) hiddenAddress.value = address;
        if (hiddenLat && coords) hiddenLat.value = coords[0];
        if (hiddenLng && coords) hiddenLng.value = coords[1];

        // Сбрасываем выбор существующей точки — используется новый адрес
        var spHidden = document.getElementById('servicePointValue');
        if (spHidden) spHidden.value = '';

        // Убираем текст ошибки валидации если был
        var validationSpan = document.querySelector('[data-valmsg-for="ServicePointId"]');
        if (validationSpan) validationSpan.textContent = '';

        // Показываем адрес и координаты под картой
        var addrBlock = document.getElementById('selectedAddress');
        var addrText = document.getElementById('addressText');
        if (addrBlock && addrText) {
            var text = address;
            if (coords) {
                text += ' (' + coords[0].toFixed(6) + ', ' + coords[1].toFixed(6) + ')';
            }
            addrText.textContent = text;
            addrBlock.style.display = 'block';
        }
    }

    // Регистрируем рабочий интерфейс карты
    window.ticketAddressMap = {
        available: true,
        show: showMap,
        // Геокодируем адрес и ставим метку (карта инициализируется при необходимости)
        selectAddress: function (address) {
            ymaps.ready(function () { geocodeAddress(address); });
        },
        suggest: suggest
    };
})();

// === Объединённый поиск точки обслуживания + нового адреса ===
// Единое поле servicePointSearch: показывает существующие точки обслуживания
// и подсказки нового адреса с Яндекс Карт в одном выпадающем списке.
(function () {
    var searchInput = document.getElementById('servicePointSearch');
    var hiddenInput = document.getElementById('servicePointValue');
    var listEl = document.getElementById('servicePointList');
    var dataScript = document.getElementById('spOptionsData');
    var clientSearchInput = document.getElementById('clientSearch');
    if (!searchInput || !hiddenInput || !listEl || !dataScript) return;

    var allOptions = [];
    try { allOptions = JSON.parse(dataScript.textContent); } catch (e) { return; }

    // Текущие опции (могут быть отфильтрованы по клиенту)
    var currentOptions = allOptions;
    // Текущие подсказки нового адреса с карты
    var currentSuggestions = [];
    var suggestTimer = null;

    // Если уже есть выбранное значение — показать текст
    if (hiddenInput.value) {
        var sel = allOptions.find(function (o) { return o.Id == hiddenInput.value; });
        if (sel) searchInput.value = sel.Text;
    }

    var activeIndex = -1;

    function escapeHtml(s) {
        var d = document.createElement('div');
        d.textContent = s;
        return d.innerHTML;
    }

    function clearNewAddress() {
        var h = document.getElementById('newAddress');
        if (h) h.value = '';
        var lat = document.getElementById('newLatitude');
        if (lat) lat.value = '';
        var lng = document.getElementById('newLongitude');
        if (lng) lng.value = '';
        var addrBlock = document.getElementById('selectedAddress');
        if (addrBlock) addrBlock.style.display = 'none';
    }

    function clearSelection() {
        hiddenInput.value = '';
        clearNewAddress();
    }

    // Выбор существующей точки обслуживания
    function selectOption(opt) {
        searchInput.value = opt.Text;
        hiddenInput.value = opt.Id;
        hideList();
        // Сбрасываем новый адрес при выборе существующей точки
        clearNewAddress();
    }

    // Выбор нового адреса (подсказка с карты) — делегируем модулю карты
    function selectNewAddress(value) {
        hideList();
        if (window.ticketAddressMap && window.ticketAddressMap.available) {
            window.ticketAddressMap.show();
            window.ticketAddressMap.selectAddress(value);
        } else {
            searchInput.value = value;
        }
    }

    function render() {
        listEl.innerHTML = '';
        activeIndex = -1;

        var query = searchInput.value.trim().toLowerCase();
        var existing;
        if (query.length === 0) {
            existing = currentOptions.slice(0, 50);
        } else {
            existing = currentOptions.filter(function (o) {
                return o.Text.toLowerCase().indexOf(query) !== -1;
            }).slice(0, 50);
        }

        var hasAny = false;

        // Существующие точки обслуживания
        existing.forEach(function (opt) {
            var btn = document.createElement('button');
            btn.type = 'button';
            btn.className = 'list-group-item list-group-item-action small py-2';
            btn.textContent = opt.Text;
            btn.addEventListener('mousedown', function (e) {
                e.preventDefault();
                selectOption(opt);
            });
            listEl.appendChild(btn);
            hasAny = true;
        });

        // Подсказки нового адреса с карты
        if (currentSuggestions.length) {
            var header = document.createElement('div');
            header.className = 'list-group-item text-muted small py-1 fw-semibold';
            header.innerHTML = '<i class="bi bi-geo-alt"></i> Новый адрес на карте';
            listEl.appendChild(header);

            currentSuggestions.forEach(function (item) {
                var btn = document.createElement('button');
                btn.type = 'button';
                btn.className = 'list-group-item list-group-item-action small py-2';
                btn.innerHTML = '<i class="bi bi-pin-map text-primary"></i> ' + escapeHtml(item.displayName);
                btn.addEventListener('mousedown', function (e) {
                    e.preventDefault();
                    selectNewAddress(item.value);
                });
                listEl.appendChild(btn);
                hasAny = true;
            });
        }

        if (!hasAny) {
            var empty = document.createElement('div');
            empty.className = 'list-group-item text-muted small py-2';
            empty.textContent = 'Ничего не найдено';
            listEl.appendChild(empty);
        }

        listEl.style.display = 'block';
    }

    function hideList() {
        listEl.style.display = 'none';
        listEl.innerHTML = '';
        activeIndex = -1;
    }

    // Запрос подсказок адреса с карты (с задержкой) и перерисовка списка
    function scheduleSuggest() {
        clearTimeout(suggestTimer);
        var query = searchInput.value.trim();
        if (!window.ticketAddressMap || !window.ticketAddressMap.available || query.length < 3) {
            currentSuggestions = [];
            return;
        }
        suggestTimer = setTimeout(function () {
            var q = searchInput.value.trim();
            if (q.length < 3) { currentSuggestions = []; return; }
            window.ticketAddressMap.suggest(q).then(function (items) {
                // Игнорируем результат, если запрос уже изменился
                if (searchInput.value.trim() !== q) return;
                currentSuggestions = items || [];
                if (listEl.style.display !== 'none') render();
            });
        }, 350);
    }

    // Загрузка точек обслуживания по выбранному клиенту
    function loadServicePointsByClient(clientId) {
        // Сбрасываем текущий выбор точки
        searchInput.value = '';
        clearSelection();
        currentSuggestions = [];

        if (!clientId) {
            // Клиент не выбран — показываем все точки
            currentOptions = allOptions;
            return;
        }

        fetch('/api/clients/' + clientId + '/service-points')
            .then(function (r) { return r.json(); })
            .then(function (data) {
                currentOptions = (data || []).map(function (sp) {
                    return { Id: sp.id, Text: sp.text };
                });
            })
            .catch(function () {
                currentOptions = allOptions;
            });
    }

    // Обработчик изменения клиента (событие из блока поиска клиента)
    if (clientSearchInput) {
        clientSearchInput.addEventListener('clientChanged', function (e) {
            loadServicePointsByClient(e.detail.clientId);
        });
    }

    searchInput.addEventListener('focus', function () {
        render();
    });

    searchInput.addEventListener('input', function () {
        clearSelection();
        currentSuggestions = [];
        render();
        scheduleSuggest();
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
