const test = require('node:test');
const assert = require('node:assert/strict');
const fs = require('node:fs');
const path = require('node:path');
const vm = require('node:vm');
const root = path.resolve(__dirname, '../../src/Notenokand.Web/wwwroot');

function worker(fetchImpl) {
    const events = {}, added = [], deleted = [];
    const fallback = new Response('offline-page');
    const context = {
        URL, Request, Response,
        self: {
            registration: {scope:'https://notenokand.test/'},
            location: {origin:'https://notenokand.test'},
            clients: {claim:async () => {}},
            skipWaiting: () => {},
            addEventListener: (name, handler) => events[name] = handler
        },
        caches: {
            open: async () => ({
                add: async request => added.push(request.url),
                match: async url => { assert.equal(url,'https://notenokand.test/offline.html'); return fallback; }
            }),
            keys: async () => ['other-app-v1','notenokand-offline-v0','notenokand-offline-v1'],
            delete: async name => deleted.push(name)
        },
        fetch: fetchImpl
    };
    vm.runInNewContext(fs.readFileSync(path.join(root,'sw.js'),'utf8'), context);
    return {events, added, deleted};
}
test('manifest points to valid 192/512 PNG icons and standalone dashboard launch', () => {
    const manifest = JSON.parse(fs.readFileSync(path.join(root,'manifest.webmanifest'),'utf8'));
    assert.equal(manifest.display,'standalone');
    assert.equal(manifest.start_url,'./app');
    assert.equal(manifest.scope,'./');
    for (const icon of manifest.icons) {
        const png = fs.readFileSync(path.join(root,icon.src));
        const [w,h] = icon.sizes.split('x').map(Number);
        assert.equal(png.readUInt32BE(16),w); assert.equal(png.readUInt32BE(20),h);
        assert.equal(png.subarray(1,4).toString(),'PNG');
    }
    assert.ok(manifest.icons.some(x => x.sizes === '192x192'));
    assert.ok(manifest.icons.some(x => x.sizes === '512x512'));
});
test('install caches only public offline document and activation preserves unrelated caches', async () => {
    const w = worker(async () => new Response('network'));
    let pending;
    w.events.install({waitUntil: promise => pending = promise}); await pending;
    assert.deepEqual(w.added,['https://notenokand.test/offline.html']);
    w.events.activate({waitUntil: promise => pending = promise}); await pending;
    assert.deepEqual(w.deleted,['notenokand-offline-v0']);
});
test('POST, API, receipts and cross-origin requests are not intercepted', () => {
    const w = worker(() => { throw new Error('should not fetch'); });
    for (const request of [
        {method:'POST',mode:'navigate',url:'https://notenokand.test/app/stock/sell'},
        {method:'GET',mode:'cors',url:'https://notenokand.test/api/locations'},
        {method:'GET',mode:'same-origin',url:'https://notenokand.test/app/finance/receipts/1'},
        {method:'GET',mode:'navigate',url:'https://external.test/'}
    ]) {
        w.events.fetch({request,respondWith: () => assert.fail('must not intercept')});
    }
});
test('authenticated navigation is network-only and never added to cache', async () => {
    const w = worker(async () => new Response('private-finance-data'));
    let pending;
    w.events.fetch({request:{method:'GET',mode:'navigate',url:'https://notenokand.test/app/finance'},respondWith:p => pending=p});
    assert.equal(await (await pending).text(),'private-finance-data');
    assert.deepEqual(w.added,[]);
});
test('failed GET navigation receives generic offline page without stored account data', async () => {
    const w = worker(async () => { throw new Error('offline'); });
    let pending;
    w.events.fetch({request:{method:'GET',mode:'navigate',url:'https://notenokand.test/app'},respondWith:p => pending=p});
    assert.equal(await (await pending).text(),'offline-page');
    assert.deepEqual(w.added,[]);
});
