// Offline-first service worker for the published app, following the Blazor WebAssembly PWA
// template: on install, cache every asset in the build manifest (the app, its content files and
// translations); on fetch, serve navigation requests from the cached index.html and everything
// else from the cache first. A new build has a new manifest hash, so it installs into a new cache
// and the old one is removed on activation. Citiz sends nothing anywhere; this file is only about
// making the study material available when the network is not.
self.importScripts('./service-worker-assets.js');

self.addEventListener('install', event => event.waitUntil(onInstall(event)));
self.addEventListener('activate', event => event.waitUntil(onActivate(event)));
self.addEventListener('fetch', event => event.respondWith(onFetch(event)));

const cacheNamePrefix = 'citiz-offline-';
const cacheName = `${cacheNamePrefix}${self.assetsManifest.version}`;
const offlineAssetsInclude = [/\.dll$/, /\.pdb$/, /\.wasm/, /\.html/, /\.js$/, /\.json$/, /\.css$/, /\.woff2?$/, /\.png$/, /\.svg$/, /\.ico$/, /\.webmanifest$/, /\.dat$/, /\.blat$/];
const offlineAssetsExclude = [/^service-worker\.js$/];

// Project sites on GitHub Pages live under /<repo>/, so the base path is read from the page.
const base = "/";
const baseUrl = new URL(base, self.origin);
const manifestUrlList = self.assetsManifest.assets.map(asset => new URL(asset.url, baseUrl).href);

async function onInstall(event) {
  console.info('Citiz service worker: install');
  const assetsRequests = self.assetsManifest.assets
    .filter(asset => offlineAssetsInclude.some(pattern => pattern.test(asset.url)))
    .filter(asset => !offlineAssetsExclude.some(pattern => pattern.test(asset.url)))
    .map(asset => new Request(asset.url, { integrity: asset.hash, cache: 'no-cache' }));
  await caches.open(cacheName).then(cache => cache.addAll(assetsRequests));
}

async function onActivate(event) {
  console.info('Citiz service worker: activate');
  const cacheKeys = await caches.keys();
  await Promise.all(cacheKeys
    .filter(key => key.startsWith(cacheNamePrefix) && key !== cacheName)
    .map(key => caches.delete(key)));
}

async function onFetch(event) {
  let cachedResponse = null;
  if (event.request.method === 'GET') {
    const shouldServeIndexHtml = event.request.mode === 'navigate'
      && !manifestUrlList.some(url => url === event.request.url);
    const request = shouldServeIndexHtml ? 'index.html' : event.request;
    const cache = await caches.open(cacheName);
    cachedResponse = await cache.match(request);
  }
  return cachedResponse || fetch(event.request);
}
