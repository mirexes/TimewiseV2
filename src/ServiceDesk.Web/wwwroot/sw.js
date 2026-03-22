// Service Worker для PWA и офлайн-доступа

const CACHE_NAME = 'teamwise-v1';
const STATIC_ASSETS = [
    '/',
    '/css/site.css',
    '/css/themes/_variables.css',
    '/css/themes/theme-overrides.css',
    '/js/site.js',
    '/js/theme-switcher.js',
    '/manifest.json'
];

// Установка — кэширование статических ресурсов
self.addEventListener('install', function (event) {
    event.waitUntil(
        caches.open(CACHE_NAME).then(function (cache) {
            return cache.addAll(STATIC_ASSETS);
        })
    );
    self.skipWaiting();
});

// Активация — очистка старых кэшей
self.addEventListener('activate', function (event) {
    event.waitUntil(
        caches.keys().then(function (keys) {
            return Promise.all(
                keys.filter(function (key) { return key !== CACHE_NAME; })
                    .map(function (key) { return caches.delete(key); })
            );
        })
    );
    self.clients.claim();
});

// Стратегия: Network First, fallback to cache
self.addEventListener('fetch', function (event) {
    // Пропускаем POST-запросы и API
    if (event.request.method !== 'GET' || event.request.url.includes('/api/')) {
        return;
    }

    event.respondWith(
        fetch(event.request)
            .then(function (response) {
                // Кэшируем успешный ответ
                if (response.ok) {
                    var clone = response.clone();
                    caches.open(CACHE_NAME).then(function (cache) {
                        cache.put(event.request, clone);
                    });
                }
                return response;
            })
            .catch(function () {
                // Возвращаем из кэша при отсутствии сети
                return caches.match(event.request);
            })
    );
});

// Push-уведомления
self.addEventListener('push', function (event) {
    var data = {};
    if (event.data) {
        data = event.data.json();
    }

    event.waitUntil(
        self.registration.showNotification(data.title || 'TEAMWISE', {
            body: data.message || '',
            icon: '/icons/icon-192.png',
            badge: '/icons/icon-192.png',
            data: { url: data.url || '/' }
        })
    );
});

// Клик по уведомлению — открываем URL
self.addEventListener('notificationclick', function (event) {
    event.notification.close();
    var url = event.notification.data.url || '/';
    event.waitUntil(
        clients.openWindow(url)
    );
});
