// Web Push подписка (VAPID) и запрос разрешения на уведомления

// Инициализация при загрузке страницы
document.addEventListener('DOMContentLoaded', function () {
    initPushNotifications();
});

// Основная функция инициализации Push-уведомлений
async function initPushNotifications() {
    if (!('serviceWorker' in navigator) || !('PushManager' in window)) {
        return;
    }

    // Регистрируем Service Worker
    try {
        await navigator.serviceWorker.register('/sw.js');
    } catch (e) {
        console.error('Ошибка регистрации Service Worker:', e);
        return;
    }

    // Если разрешение уже дано — подписываемся
    if (Notification.permission === 'granted') {
        await subscribeToPush();
        return;
    }

    // Если разрешение ещё не запрашивалось — запрашиваем
    if (Notification.permission === 'default') {
        requestNotificationPermission();
    }
}

// Запрос разрешения на уведомления
async function requestNotificationPermission() {
    try {
        var permission = await Notification.requestPermission();
        if (permission === 'granted') {
            await subscribeToPush();
        }
    } catch (e) {
        console.error('Ошибка запроса разрешения:', e);
    }
}

// Подписка на Push-уведомления
async function subscribeToPush() {
    // Получаем VAPID ключ с сервера
    var vapidKey = window.VAPID_PUBLIC_KEY;
    if (!vapidKey) {
        try {
            var resp = await fetch('/api/notifications/vapid-key');
            if (resp.ok) {
                var data = await resp.json();
                vapidKey = data.key;
                window.VAPID_PUBLIC_KEY = vapidKey;
            }
        } catch (e) { /* игнорируем */ }
    }
    if (!vapidKey) return;

    try {
        var registration = await navigator.serviceWorker.ready;

        // Проверяем, есть ли уже подписка
        var existing = await registration.pushManager.getSubscription();
        if (existing) return;

        var subscription = await registration.pushManager.subscribe({
            userVisibleOnly: true,
            applicationServerKey: urlBase64ToUint8Array(vapidKey)
        });

        // Отправляем подписку на сервер
        await fetch('/api/notifications/subscribe', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({
                endpoint: subscription.endpoint,
                p256dh: arrayBufferToBase64(subscription.getKey('p256dh')),
                auth: arrayBufferToBase64(subscription.getKey('auth'))
            })
        });
    } catch (e) {
        console.error('Ошибка подписки на Push:', e);
    }
}

// Преобразование VAPID ключа из Base64URL в Uint8Array
function urlBase64ToUint8Array(base64String) {
    var padding = '='.repeat((4 - base64String.length % 4) % 4);
    var base64 = (base64String + padding).replace(/-/g, '+').replace(/_/g, '/');
    var rawData = atob(base64);
    var outputArray = new Uint8Array(rawData.length);
    for (var i = 0; i < rawData.length; i++) {
        outputArray[i] = rawData.charCodeAt(i);
    }
    return outputArray;
}

// Преобразование ArrayBuffer в Base64 строку
function arrayBufferToBase64(buffer) {
    var bytes = new Uint8Array(buffer);
    var binary = '';
    for (var i = 0; i < bytes.byteLength; i++) {
        binary += String.fromCharCode(bytes[i]);
    }
    return btoa(binary);
}
