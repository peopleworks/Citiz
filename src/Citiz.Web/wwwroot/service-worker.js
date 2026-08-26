// In development, always fetch from the network and do not enable offline support, so a code or
// content change shows up on the next reload. The published worker (service-worker.published.js)
// is the one that caches for offline use.
self.addEventListener('fetch', () => { });
