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

// === Яндекс Карты — выбор нового адреса ===
(function () {
    const btnShow = document.getElementById('btnShowMap');
    const mapContainer = document.getElementById('mapContainer');
    if (!btnShow || !mapContainer) return;

    // Проверяем доступность ymaps
    if (typeof ymaps === 'undefined') return;

    let mapInitialized = false;
    let yMap = null;
    let placemark = null;

    btnShow.addEventListener('click', function () {
        if (mapContainer.style.display === 'none') {
            mapContainer.style.display = 'block';
            btnShow.innerHTML = '<i class="bi bi-x-lg"></i> Скрыть карту';

            if (!mapInitialized) {
                ymaps.ready(initMap);
                mapInitialized = true;
            }
        } else {
            mapContainer.style.display = 'none';
            btnShow.innerHTML = '<i class="bi bi-geo-alt"></i> Указать новый адрес на карте';
        }
    });

    function initMap() {
        yMap = new ymaps.Map('yandexMap', {
            center: [55.751574, 37.573856], // Москва
            zoom: 10,
            controls: ['zoomControl', 'geolocationControl']
        });

        // Клик по карте — установка метки
        yMap.events.add('click', function (e) {
            var coords = e.get('coords');
            setPlacemark(coords);
            reverseGeocode(coords);
        });

        // Поиск по адресу
        var searchInput = document.getElementById('addressSearch');
        if (searchInput) {
            var suggestView = new ymaps.SuggestView('addressSearch');

            searchInput.addEventListener('keydown', function (e) {
                if (e.key === 'Enter') {
                    e.preventDefault();
                    geocodeAddress(searchInput.value);
                }
            });

            // При выборе подсказки — геокодируем
            suggestView.events.add('select', function (e) {
                var selectedValue = e.get('item').value;
                geocodeAddress(selectedValue);
            });
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
            var address = firstGeoObject.getAddressLine();
            showAddress(address);
        });
    }

    function geocodeAddress(address) {
        if (!address) return;
        ymaps.geocode(address, { results: 1 }).then(function (res) {
            var firstGeoObject = res.geoObjects.get(0);
            if (!firstGeoObject) return;

            var coords = firstGeoObject.geometry.getCoordinates();
            var resolvedAddress = firstGeoObject.getAddressLine();

            setPlacemark(coords);
            yMap.setCenter(coords, 16);
            showAddress(resolvedAddress);
        });
    }

    function showAddress(address) {
        var addrBlock = document.getElementById('selectedAddress');
        var addrText = document.getElementById('addressText');
        if (addrBlock && addrText) {
            addrText.textContent = address;
            addrBlock.style.display = 'block';
        }
    }
})();
