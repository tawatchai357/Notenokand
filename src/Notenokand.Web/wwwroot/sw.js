'use strict';
// Cache only this public offline document. Never cache HTML from signed-in pages,
// receipts, API responses, POST requests, or financial/user data.
const CACHE_NAME = 'notenokand-offline-v1';
const OFFLINE_URL = new URL('offline.html', self.registration.scope).href;
self.addEventListener('install', event => {
    event.waitUntil(caches.open(CACHE_NAME).then(cache => cache.add(new Request(OFFLINE_URL, {cache: 'reload'}))));
});
self.addEventListener('activate', event => {
    event.waitUntil((async () => {
        for (const key of await caches.keys()) {
            if (key.startsWith('notenokand-offline-') && key !== CACHE_NAME) await caches.delete(key);
        }
        await self.clients.claim();
    })());
});
self.addEventListener('message', event => {
    if (event.data?.type === 'ACTIVATE_UPDATE') self.skipWaiting();
});
self.addEventListener('fetch', event => {
    const request = event.request;
    if (request.method !== 'GET' || request.mode !== 'navigate' ||
        new URL(request.url).origin !== self.location.origin) return;
    event.respondWith(fetch(request).catch(async () =>
        (await caches.open(CACHE_NAME)).match(OFFLINE_URL).then(response =>
            response || new Response('Offline. Please reconnect and try again.', {
                status: 503, headers: {'Content-Type': 'text/plain; charset=utf-8'}
            }))));
});
