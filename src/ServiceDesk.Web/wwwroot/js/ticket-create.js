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

// === Поиск по точкам обслуживания с фильтрацией ===
(function () {
    var searchInput = document.getElementById('servicePointSearch');
    var hiddenInput = document.getElementById('servicePointValue');
    var listEl = document.getElementById('servicePointList');
    var dataScript = document.getElementById('spOptionsData');
    var clientSelect = document.getElementById('clientSelect');
    if (!searchInput || !hiddenInput || !listEl || !dataScript) return;

    var allOptions = [];
    try { allOptions = JSON.parse(dataScript.textContent); } catch (e) { return; }

    // Текущие опции (могут быть отфильтрованы по клиенту)
    var currentOptions = allOptions;

    // Если уже есть выбранное значение — показать текст
    if (hiddenInput.value) {
        var sel = allOptions.find(function (o) { return o.Id == hiddenInput.value; });
        if (sel) searchInput.value = sel.Text;
    }

    var activeIndex = -1;

    function clearSelection() {
        hiddenInput.value = '';
        // Сбрасываем новый адрес
        var h = document.getElementById('newAddress');
        if (h) h.value = '';
        var lat = document.getElementById('newLatitude');
        if (lat) lat.value = '';
        var lng = document.getElementById('newLongitude');
        if (lng) lng.value = '';
    }

    function selectOption(opt) {
        searchInput.value = opt.Text;
        hiddenInput.value = opt.Id;
        hideList();
        // Сбрасываем новый адрес при выборе существующей точки
        var h = document.getElementById('newAddress');
        if (h) h.value = '';
        var lat = document.getElementById('newLatitude');
        if (lat) lat.value = '';
        var lng = document.getElementById('newLongitude');
        if (lng) lng.value = '';
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
        items.forEach(function (opt, idx) {
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
            filtered = currentOptions.slice(0, 50);
        } else {
            filtered = currentOptions.filter(function (o) {
                return o.Text.toLowerCase().indexOf(query) !== -1;
            }).slice(0, 50);
        }
        renderList(filtered);
    }

    // Загрузка точек обслуживания по выбранному клиенту
    function loadServicePointsByClient(clientId) {
        // Сбрасываем текущий выбор точки
        searchInput.value = '';
        clearSelection();

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

    // Обработчик изменения клиента
    if (clientSelect) {
        clientSelect.addEventListener('change', function () {
            loadServicePointsByClient(clientSelect.value);
        });
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

// === Яндекс Карты — выбор нового адреса ===
(function () {
    const btnShow = document.getElementById('btnShowMap');
    const mapContainer = document.getElementById('mapContainer');
    if (!btnShow || !mapContainer) return;

    if (typeof ymaps === 'undefined') {
        console.warn('[ticket-create] ymaps не загружен — проверьте API-ключ Яндекс Карт');
        return;
    }

    let mapInitialized = false;
    let yMap = null;
    let placemark = null;

    btnShow.addEventListener('click', function () {
        if (mapContainer.style.display === 'none') {
            mapContainer.style.display = 'block';
            btnShow.innerHTML = '<i class="bi bi-x-lg"></i> Скрыть карту';

            if (!mapInitialized) {
                mapInitialized = true;
                ymaps.ready(initMap);
            }
        } else {
            mapContainer.style.display = 'none';
            btnShow.innerHTML = '<i class="bi bi-geo-alt"></i> Указать новый адрес на карте';
        }
    });

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

        initAddressSearch();
    }

    function initAddressSearch() {
        var searchInput = document.getElementById('addressSearch');
        var suggestList = document.getElementById('suggestList');
        if (!searchInput || !suggestList) {
            console.warn('[ticket-create] addressSearch или suggestList не найдены в DOM');
            return;
        }

        var debounceTimer = null;

        searchInput.addEventListener('input', function () {
            clearTimeout(debounceTimer);
            var query = searchInput.value.trim();

            if (query.length < 3) {
                hideSuggestions();
                return;
            }

            debounceTimer = setTimeout(function () {
                // Пробуем ymaps.suggest, если не работает — fallback на ymaps.geocode
                fetchSuggestions(query);
            }, 350);
        });

        searchInput.addEventListener('keydown', function (e) {
            if (e.key === 'Enter') {
                e.preventDefault();
                hideSuggestions();
                geocodeAddress(searchInput.value);
            }
        });

        // Скрываем подсказки при клике вне
        document.addEventListener('mousedown', function (e) {
            if (!searchInput.contains(e.target) && !suggestList.contains(e.target)) {
                hideSuggestions();
            }
        });

        function fetchSuggestions(query) {
            // Используем ymaps.suggest если доступен, иначе geocode
            var suggestPromise = (typeof ymaps.suggest === 'function')
                ? ymaps.suggest(query)
                : null;

            if (suggestPromise) {
                suggestPromise.then(function (items) {
                    renderSuggestItems(items);
                }).catch(function () {
                    // Fallback на geocode если suggest не сработал
                    geocodeFallback(query);
                });
            } else {
                geocodeFallback(query);
            }
        }

        function geocodeFallback(query) {
            ymaps.geocode(query, { results: 5 }).then(function (res) {
                var items = [];
                res.geoObjects.each(function (obj) {
                    items.push({
                        displayName: obj.getAddressLine(),
                        value: obj.getAddressLine()
                    });
                });
                renderSuggestItems(items);
            }).catch(function (err) {
                console.error('[ticket-create] Ошибка геокодирования:', err);
                hideSuggestions();
            });
        }

        function renderSuggestItems(items) {
            suggestList.innerHTML = '';
            if (!items || items.length === 0) {
                hideSuggestions();
                return;
            }
            items.forEach(function (item) {
                var btn = document.createElement('button');
                btn.type = 'button';
                btn.className = 'list-group-item list-group-item-action small py-2';
                btn.textContent = item.displayName;
                btn.addEventListener('mousedown', function (e) {
                    e.preventDefault(); // Не даём blur сработать раньше клика
                    searchInput.value = item.value;
                    hideSuggestions();
                    geocodeAddress(item.value);
                });
                suggestList.appendChild(btn);
            });
            suggestList.style.display = 'block';
        }

        function hideSuggestions() {
            suggestList.style.display = 'none';
            suggestList.innerHTML = '';
        }
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
            setPlacemark(coords);
            yMap.setCenter(coords, 16);
            showAddress(firstGeoObject.getAddressLine(), coords);
        });
    }

    function showAddress(address, coords) {
        // Подставляем адрес в поле поиска
        var searchInput = document.getElementById('addressSearch');
        if (searchInput) {
            searchInput.value = address;
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
        var spSearch = document.getElementById('servicePointSearch');
        if (spHidden) spHidden.value = '';
        if (spSearch) {
            spSearch.value = '';
            spSearch.classList.remove('input-validation-error');
        }

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
})();
