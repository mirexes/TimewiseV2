// PWA — баннер установки приложения
(function () {
    'use strict';

    let deferredPrompt = null;
    const DISMISS_KEY = 'pwa-install-dismissed';
    const DISMISS_DAYS = 7; // Показывать снова через 7 дней после закрытия

    // Проверяем, не отклонил ли пользователь баннер недавно
    function isDismissed() {
        const dismissed = localStorage.getItem(DISMISS_KEY);
        if (!dismissed) return false;
        const dismissedAt = parseInt(dismissed, 10);
        const daysPassed = (Date.now() - dismissedAt) / (1000 * 60 * 60 * 24);
        return daysPassed < DISMISS_DAYS;
    }

    // Проверяем, установлено ли уже приложение
    function isInstalled() {
        // standalone-режим — приложение уже установлено
        if (window.matchMedia('(display-mode: standalone)').matches) return true;
        if (window.navigator.standalone === true) return true;
        return false;
    }

    // Создаём баннер установки
    function createBanner() {
        const banner = document.createElement('div');
        banner.id = 'pwaInstallBanner';
        banner.className = 'pwa-install-banner';
        banner.innerHTML =
            '<div class="pwa-install-banner-content">' +
                '<div class="pwa-install-banner-icon">' +
                    '<img src="/icons/icon-192.png" alt="TEAMWISE" onerror="this.style.display=\'none\';this.nextElementSibling.style.display=\'flex\'" />' +
                    '<div class="pwa-install-banner-icon-fallback" style="display:none"><i class="bi bi-phone"></i></div>' +
                '</div>' +
                '<div class="pwa-install-banner-text">' +
                    '<div class="pwa-install-banner-title">Установить TEAMWISE</div>' +
                    '<div class="pwa-install-banner-desc">Добавьте приложение на главный экран для быстрого доступа</div>' +
                '</div>' +
            '</div>' +
            '<div class="pwa-install-banner-actions">' +
                '<button class="pwa-install-banner-close" id="pwaInstallDismiss" title="Закрыть">&times;</button>' +
                '<button class="btn btn-primary btn-sm pwa-install-banner-btn" id="pwaInstallBtn">Установить</button>' +
            '</div>';
        document.body.appendChild(banner);

        // Кнопка «Установить»
        document.getElementById('pwaInstallBtn').addEventListener('click', async function () {
            if (!deferredPrompt) return;
            deferredPrompt.prompt();
            const result = await deferredPrompt.userChoice;
            if (result.outcome === 'accepted') {
                hideBanner();
            }
            deferredPrompt = null;
        });

        // Кнопка «Закрыть»
        document.getElementById('pwaInstallDismiss').addEventListener('click', function () {
            localStorage.setItem(DISMISS_KEY, Date.now().toString());
            hideBanner();
        });

        // Показываем с анимацией
        requestAnimationFrame(function () {
            requestAnimationFrame(function () {
                banner.classList.add('show');
            });
        });
    }

    // Создаём подсказку для iOS Safari (не поддерживает beforeinstallprompt)
    function createIOSBanner() {
        const banner = document.createElement('div');
        banner.id = 'pwaInstallBanner';
        banner.className = 'pwa-install-banner';
        banner.innerHTML =
            '<div class="pwa-install-banner-content">' +
                '<div class="pwa-install-banner-icon">' +
                    '<img src="/icons/icon-192.png" alt="TEAMWISE" onerror="this.style.display=\'none\';this.nextElementSibling.style.display=\'flex\'" />' +
                    '<div class="pwa-install-banner-icon-fallback" style="display:none"><i class="bi bi-phone"></i></div>' +
                '</div>' +
                '<div class="pwa-install-banner-text">' +
                    '<div class="pwa-install-banner-title">Установить TEAMWISE</div>' +
                    '<div class="pwa-install-banner-desc">' +
                        'Нажмите <i class="bi bi-box-arrow-up"></i> внизу экрана, затем «На экран "Домой"»' +
                    '</div>' +
                '</div>' +
            '</div>' +
            '<div class="pwa-install-banner-actions">' +
                '<button class="pwa-install-banner-close" id="pwaInstallDismiss" title="Закрыть">&times;</button>' +
            '</div>';
        document.body.appendChild(banner);

        document.getElementById('pwaInstallDismiss').addEventListener('click', function () {
            localStorage.setItem(DISMISS_KEY, Date.now().toString());
            hideBanner();
        });

        requestAnimationFrame(function () {
            requestAnimationFrame(function () {
                banner.classList.add('show');
            });
        });
    }

    function hideBanner() {
        var banner = document.getElementById('pwaInstallBanner');
        if (!banner) return;
        banner.classList.remove('show');
        banner.addEventListener('transitionend', function () {
            banner.remove();
        });
    }

    // Определяем iOS Safari
    function isIOSSafari() {
        var ua = navigator.userAgent;
        var isIOS = /iPad|iPhone|iPod/.test(ua) || (navigator.platform === 'MacIntel' && navigator.maxTouchPoints > 1);
        var isSafari = /Safari/.test(ua) && !/CriOS|FxiOS|OPiOS|EdgiOS/.test(ua);
        return isIOS && isSafari;
    }

    // Перехватываем событие beforeinstallprompt (Chrome, Edge, Samsung Internet)
    window.addEventListener('beforeinstallprompt', function (e) {
        e.preventDefault();
        deferredPrompt = e;

        if (isInstalled() || isDismissed()) return;

        createBanner();
    });

    // Скрываем баннер после установки
    window.addEventListener('appinstalled', function () {
        hideBanner();
        deferredPrompt = null;
    });

    // Для iOS Safari показываем инструкцию
    document.addEventListener('DOMContentLoaded', function () {
        if (isInstalled() || isDismissed()) return;

        if (isIOSSafari()) {
            createIOSBanner();
        }
    });
})();
