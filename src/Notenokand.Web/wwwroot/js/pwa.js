(() => {
    const script = document.currentScript;
    const swUrl = script?.dataset.swUrl;
    const standalone = window.matchMedia('(display-mode: standalone)');
    let promptEvent = null, registration = null, requestedReload = false;
    const installed = () => standalone.matches || window.navigator.standalone === true;
    function updateInstallUi() {
        document.querySelectorAll('[data-install-pwa]').forEach(el => el.hidden = !promptEvent || installed());
        document.querySelectorAll('[data-pwa-installed]').forEach(el => el.hidden = !installed());
        document.querySelectorAll('[data-pwa-insecure]').forEach(el => el.hidden = window.isSecureContext);
    }
    window.addEventListener('beforeinstallprompt', event => {
        event.preventDefault(); promptEvent = event; updateInstallUi();
    });
    window.addEventListener('appinstalled', () => { promptEvent = null; updateInstallUi(); });
    standalone.addEventListener?.('change', updateInstallUi);
    document.querySelectorAll('[data-install-pwa]').forEach(button => button.addEventListener('click', async () => {
        if (!promptEvent) return;
        const current = promptEvent; promptEvent = null; updateInstallUi();
        try { await current.prompt(); await current.userChoice; } catch { /* Browser may revoke the prompt. */ }
    }));
    const connectivity = () => {
        document.querySelectorAll('[data-pwa-offline]').forEach(el => el.hidden = navigator.onLine);
    };
    window.addEventListener('online', connectivity);
    window.addEventListener('offline', connectivity);
    // Do not queue/replay writes. Keep the current form visible for the user.
    document.addEventListener('submit', event => {
        if (!navigator.onLine && event.target instanceof HTMLFormElement &&
            event.target.method.toLowerCase() !== 'get') {
            event.preventDefault(); event.stopImmediatePropagation();
            window.alert('ขณะนี้ออฟไลน์ กรุณาเชื่อมต่ออินเทอร์เน็ตก่อนบันทึก ข้อมูลในแบบฟอร์มยังอยู่ในหน้านี้');
        }
    }, true);
    const updateBanner = document.querySelector('[data-pwa-update]');
    document.querySelector('[data-pwa-update-dismiss]')?.addEventListener('click', () => { if (updateBanner) updateBanner.hidden = true; });
    document.querySelector('[data-pwa-update-button]')?.addEventListener('click', () => {
        if (!registration?.waiting || !window.confirm('บันทึกงานที่ค้างไว้แล้วหรือยัง? การอัปเดตจะโหลดหน้าใหม่และข้อมูลที่ยังไม่บันทึกอาจหายไป')) return;
        requestedReload = true;
        registration.waiting.postMessage({type: 'ACTIVATE_UPDATE'});
    });
    updateInstallUi(); connectivity();
    if ('serviceWorker' in navigator && window.isSecureContext && swUrl) {
        navigator.serviceWorker.addEventListener('controllerchange', () => {
            if (requestedReload) { requestedReload = false; window.location.reload(); }
        });
        window.addEventListener('load', async () => {
            try {
                registration = await navigator.serviceWorker.register(swUrl, {updateViaCache: 'none'});
                const showUpdate = () => { if (registration.waiting && updateBanner) updateBanner.hidden = false; };
                showUpdate();
                registration.addEventListener('updatefound', () => {
                    const worker = registration.installing;
                    worker?.addEventListener('statechange', showUpdate);
                });
            } catch (error) { console.warn('PWA registration unavailable', error); }
        });
    }
})();
