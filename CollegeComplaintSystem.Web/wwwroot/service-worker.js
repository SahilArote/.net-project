const CACHE_NAME = 'ccms-pwa-v2';
const PRECACHE_ASSETS = [
    '/Home/Offline',
    '/manifest.json',
    '/css/site.css',
    '/js/site.js',
    '/icons/icon.svg',
    '/icons/icon-192.png',
    '/icons/icon-512.png',
    '/icons/apple-touch-icon.png',
    '/favicon.ico'
];

// Install: precache essential offline shell assets
self.addEventListener('install', event => {
    event.waitUntil(
        caches.open(CACHE_NAME).then(async cache => {
            const results = await Promise.allSettled(
                PRECACHE_ASSETS.map(asset => cache.add(asset))
            );
            results.forEach((res, i) => {
                if (res.status === 'rejected') {
                    console.warn(`[SW] Precache failed for ${PRECACHE_ASSETS[i]}:`, res.reason);
                }
            });
        })
    );
    self.skipWaiting();
});

// Activate: clean up outdated caches
self.addEventListener('activate', event => {
    event.waitUntil(
        caches.keys().then(keys => {
            return Promise.all(
                keys.map(key => {
                    if (key !== CACHE_NAME) {
                        return caches.delete(key);
                    }
                })
            );
        }).then(() => self.clients.claim())
    );
});

// Fetch: network-first for navigation with offline fallback, cache-first for static assets
self.addEventListener('fetch', event => {
    // Only handle GET requests
    if (event.request.method !== 'GET') {
        return;
    }

    const url = new URL(event.request.url);

    // Skip non-HTTP(S) schemes
    if (!url.protocol.startsWith('http')) {
        return;
    }

    // Static assets (CSS, JS, images, icons, fonts, manifest): Stale-While-Revalidate
    const isStaticAsset = 
        url.pathname.startsWith('/css/') ||
        url.pathname.startsWith('/js/') ||
        url.pathname.startsWith('/icons/') ||
        url.pathname.startsWith('/lib/') ||
        url.pathname === '/manifest.json' ||
        url.pathname === '/favicon.ico' ||
        /\.(png|jpg|jpeg|svg|webp|gif|woff2?|ttf|eot)$/i.test(url.pathname);

    if (isStaticAsset) {
        event.respondWith(
            caches.match(event.request).then(cachedResponse => {
                const networkFetch = fetch(event.request).then(networkResponse => {
                    if (networkResponse && networkResponse.status === 200) {
                        const responseClone = networkResponse.clone();
                        caches.open(CACHE_NAME).then(cache => {
                            cache.put(event.request, responseClone);
                        });
                    }
                    return networkResponse;
                }).catch(() => cachedResponse);

                return cachedResponse || networkFetch;
            })
        );
        return;
    }

    // Navigation and HTML documents: Network-first, fallback to /Home/Offline
    if (event.request.mode === 'navigate') {
        event.respondWith(
            fetch(event.request)
                .catch(() => {
                    return caches.match('/Home/Offline').then(cachedOffline => {
                        if (cachedOffline) {
                            return cachedOffline;
                        }
                        return new Response(
                            '<!DOCTYPE html><html lang="en"><head><meta charset="utf-8"><meta name="viewport" content="width=device-width, initial-scale=1"><title>Offline - College Complaints</title><style>body{font-family:sans-serif;text-align:center;padding:3rem 1rem;background:#F8FAFC;color:#111827;}h1{font-size:1.5rem;font-weight:700;}p{color:#64748B;max-width:400px;margin:1rem auto;}a{display:inline-block;margin-top:1.5rem;background:#111827;color:#fff;padding:0.75rem 1.5rem;border-radius:0.5rem;text-decoration:none;font-weight:600;}</style></head><body><h1>You are currently offline</h1><p>An active internet connection is required. Please check your network and try again.</p><a href="/">Retry Connection</a></body></html>',
                            {
                                status: 503,
                                statusText: 'Service Unavailable',
                                headers: { 'Content-Type': 'text/html; charset=utf-8' }
                            }
                        );
                    });
                })
        );
        return;
    }

    // Other requests: Network with cache fallback
    event.respondWith(
        fetch(event.request).catch(() => caches.match(event.request))
    );
});

// Support manual skipWaiting
self.addEventListener('message', event => {
    if (event.data && event.data.action === 'skipWaiting') {
        self.skipWaiting();
    }
});
