// ==========================================
// 1. PWA Service Worker & Install Management
// ==========================================
let deferredInstallPrompt = null;

// Register service worker
if ('serviceWorker' in navigator) {
    window.addEventListener('load', () => {
        navigator.serviceWorker.register('/service-worker.js')
            .then(reg => {
                console.log('[PWA] Service Worker registered with scope:', reg.scope);
                reg.addEventListener('updatefound', () => {
                    const newWorker = reg.installing;
                    if (newWorker) {
                        newWorker.addEventListener('statechange', () => {
                            if (newWorker.state === 'installed' && navigator.serviceWorker.controller) {
                                console.log('[PWA] New content is available; please refresh.');
                            }
                        });
                    }
                });
            })
            .catch(err => console.error('[PWA] Service Worker registration failed:', err));
    });
}

// Detect if running inside installed standalone PWA
function isPwaStandalone() {
    return window.matchMedia('(display-mode: standalone)').matches ||
           window.navigator.standalone === true ||
           document.referrer.includes('android-app://');
}

// Detect client platform
function getClientPlatform() {
    const ua = navigator.userAgent || '';
    if (/iPad|iPhone|iPod/.test(ua) && !window.MSStream) return 'ios';
    if (/android/i.test(ua)) return 'android';
    if (/Macintosh|Mac OS X/i.test(ua)) return 'mac';
    if (/Windows/i.test(ua)) return 'windows';
    return 'other';
}

// Update UI elements to reflect installed state
function updatePwaInstalledUI() {
    document.querySelectorAll('.pwa-install-trigger').forEach(btn => {
        const textSpan = btn.querySelector('.pwa-btn-text');
        if (textSpan) textSpan.textContent = 'App Installed';
        btn.classList.add('opacity-80');
        btn.setAttribute('aria-label', 'Application is installed');
    });

    const platformIndicator = document.getElementById('pwa-platform-indicator');
    if (platformIndicator) {
        platformIndicator.innerHTML = '<span class="text-emerald-400 font-medium">✓ Application is installed and ready to use</span>';
    }
}

// Capture native beforeinstallprompt event (Chrome, Edge, Chromium Android)
window.addEventListener('beforeinstallprompt', (e) => {
    e.preventDefault();
    deferredInstallPrompt = e;
    console.log('[PWA] beforeinstallprompt event captured');

    document.querySelectorAll('.pwa-install-trigger').forEach(btn => {
        btn.classList.remove('hidden');
    });
});

// Capture appinstalled event
window.addEventListener('appinstalled', () => {
    console.log('[PWA] App successfully installed');
    deferredInstallPrompt = null;
    updatePwaInstalledUI();
    closePwaModal();
});

// Guide Modal functions
function openPwaModal() {
    const modal = document.getElementById('pwa-guide-modal');
    const content = document.getElementById('pwa-guide-content');
    if (!modal || !content) return;

    const platform = getClientPlatform();
    const isStandalone = isPwaStandalone();

    if (isStandalone) {
        content.innerHTML = `
            <div class="p-3 bg-emerald-50 border border-emerald-200 rounded-lg text-emerald-800 text-xs leading-relaxed">
                <strong class="font-bold text-sm block mb-1">✓ Already Installed!</strong>
                You are currently running the official College Complaints standalone application.
            </div>
        `;
    } else if (platform === 'ios') {
        content.innerHTML = `
            <p class="font-semibold text-gray-900 mb-2">To download & install on iOS Safari:</p>
            <ol class="list-decimal list-inside space-y-2.5 text-xs text-gray-700">
                <li>Tap the <strong class="text-blue-600 font-semibold">Share</strong> button at the bottom of Safari (<span class="font-mono text-gray-500">[ ↑ ]</span> icon).</li>
                <li>Scroll down the actions list and tap <strong class="text-gray-900 font-bold">"Add to Home Screen"</strong>.</li>
                <li>Tap <strong class="text-blue-600 font-bold">Add</strong> in the top-right corner to finish.</li>
            </ol>
            <p class="text-[11px] text-gray-500 mt-2">The College Complaints app icon will be added to your home screen.</p>
        `;
    } else if (platform === 'android') {
        content.innerHTML = `
            <p class="font-semibold text-gray-900 mb-2">To download & install on Android:</p>
            <ol class="list-decimal list-inside space-y-2.5 text-xs text-gray-700">
                <li>Tap the browser menu <strong class="text-gray-900 font-bold">(⋮ three dots)</strong> in the top-right corner.</li>
                <li>Select <strong class="text-blue-600 font-bold">"Install app"</strong> or <strong class="text-blue-600 font-bold">"Add to Home screen"</strong>.</li>
                <li>Tap <strong class="text-gray-900 font-bold">Install</strong> on the confirmation popup.</li>
            </ol>
        `;
    } else {
        content.innerHTML = `
            <p class="font-semibold text-gray-900 mb-2">To install on Desktop / Laptop:</p>
            <ol class="list-decimal list-inside space-y-2.5 text-xs text-gray-700">
                <li>Click the <strong class="text-blue-600 font-bold">Install icon</strong> in your browser's address bar (on the right side).</li>
                <li>Or open the browser menu <strong class="text-gray-900 font-bold">(⋮ or ...)</strong> and click <strong class="text-blue-600 font-bold">"Install College Complaints..."</strong>.</li>
                <li>Click <strong class="text-gray-900 font-bold">Install</strong> in the dialog to complete.</li>
            </ol>
        `;
    }

    modal.classList.remove('hidden');
}

function closePwaModal() {
    const modal = document.getElementById('pwa-guide-modal');
    if (modal) modal.classList.add('hidden');
}

// Global click handler for all PWA install triggers
async function handlePwaInstall() {
    if (isPwaStandalone()) {
        openPwaModal();
        return;
    }

    if (deferredInstallPrompt) {
        try {
            deferredInstallPrompt.prompt();
            const choice = await deferredInstallPrompt.userChoice;
            if (choice.outcome === 'accepted') {
                console.log('[PWA] User accepted the installation');
                updatePwaInstalledUI();
            } else {
                console.log('[PWA] User dismissed the install prompt');
            }
        } catch (err) {
            console.warn('[PWA] prompt error, opening guide', err);
            openPwaModal();
        }
        deferredInstallPrompt = null;
    } else {
        // Fallback to platform-tailored modal instructions
        openPwaModal();
    }
}

// Online/Offline Network Status Detection
function updateOnlineStatus() {
    const banner = document.getElementById('offline-banner');
    if (banner) {
        if (!navigator.onLine) {
            banner.style.display = 'block';
        } else {
            banner.style.display = 'none';
        }
    }
}

window.addEventListener('online', updateOnlineStatus);
window.addEventListener('offline', updateOnlineStatus);

// Initialize PWA triggers and guide modal listeners on DOM ready
document.addEventListener('DOMContentLoaded', () => {
    updateOnlineStatus();

    if (isPwaStandalone()) {
        updatePwaInstalledUI();
    }

    // Attach click listener to all download/install buttons
    document.querySelectorAll('.pwa-install-trigger').forEach(btn => {
        btn.addEventListener('click', (e) => {
            e.preventDefault();
            handlePwaInstall();
        });
    });

    // Modal listeners
    const modalCloseBtn = document.getElementById('pwa-modal-close');
    const modalActionBtn = document.getElementById('pwa-guide-action-btn');
    const modal = document.getElementById('pwa-guide-modal');

    if (modalCloseBtn) modalCloseBtn.addEventListener('click', closePwaModal);
    if (modalActionBtn) modalActionBtn.addEventListener('click', closePwaModal);
    if (modal) {
        modal.addEventListener('click', (e) => {
            if (e.target === modal) closePwaModal();
        });
        document.addEventListener('keydown', (e) => {
            if (e.key === 'Escape' && !modal.classList.contains('hidden')) {
                closePwaModal();
            }
        });
    }

    // Platform text indicator on home page
    const platformIndicator = document.getElementById('pwa-platform-indicator');
    if (platformIndicator && !isPwaStandalone()) {
        const plat = getClientPlatform();
        if (plat === 'windows') {
            platformIndicator.textContent = 'Instant install for Windows PC / Edge / Chrome';
        } else if (plat === 'android') {
            platformIndicator.textContent = '1-tap install for Android / Chrome';
        } else if (plat === 'ios') {
            platformIndicator.textContent = 'Add to Home Screen for iPhone / iPad';
        } else if (plat === 'mac') {
            platformIndicator.textContent = 'Instant install for Mac / Chrome / Edge';
        }
    }
});

// Mobile Navigation Toggle & Camera/Photo Picker
document.addEventListener('DOMContentLoaded', () => {
    // 1. Mobile Menu Toggle
    const mobileBtn = document.getElementById('mobile-menu-btn');
    const navLinks = document.getElementById('nav-links');
    if (mobileBtn && navLinks) {
        mobileBtn.addEventListener('click', () => {
            const isHidden = navLinks.classList.toggle('hidden');
            navLinks.classList.toggle('show', !isHidden);
            mobileBtn.setAttribute('aria-expanded', (!isHidden).toString());
        });

        // Close on Escape key
        document.addEventListener('keydown', (e) => {
            if (e.key === 'Escape' && !navLinks.classList.contains('hidden')) {
                navLinks.classList.add('hidden');
                navLinks.classList.remove('show');
                mobileBtn.setAttribute('aria-expanded', 'false');
                mobileBtn.focus();
            }
        });
    }

    // 1b. Mobile Sidebar Drawer Toggle (Portal layout)
    const mobileSidebar = document.getElementById('mobile-sidebar');
    const mobileSidebarOpen = document.getElementById('mobile-sidebar-open');
    const mobileSidebarClose = document.getElementById('mobile-sidebar-close');
    const mobileSidebarBackdrop = document.getElementById('mobile-sidebar-backdrop');

    function openMobileSidebar() {
        if (mobileSidebar) {
            mobileSidebar.classList.remove('hidden');
            document.body.classList.add('overflow-hidden');
        }
    }

    function closeMobileSidebar() {
        if (mobileSidebar) {
            mobileSidebar.classList.add('hidden');
            document.body.classList.remove('overflow-hidden');
        }
    }

    if (mobileSidebarOpen) {
        mobileSidebarOpen.addEventListener('click', openMobileSidebar);
    }
    if (mobileSidebarClose) {
        mobileSidebarClose.addEventListener('click', closeMobileSidebar);
    }
    if (mobileSidebarBackdrop) {
        mobileSidebarBackdrop.addEventListener('click', closeMobileSidebar);
    }

    document.addEventListener('keydown', (e) => {
        if (e.key === 'Escape' && mobileSidebar && !mobileSidebar.classList.contains('hidden')) {
            closeMobileSidebar();
        }
    });

    // 2. Photo Picker & Live Camera Integration
    const imageInput = document.getElementById('image-upload-input');
    const cameraDirectInput = document.getElementById('camera-direct-input');
    const btnChooseFile = document.getElementById('btn-choose-file');
    const btnOpenCamera = document.getElementById('btn-open-camera');
    const cameraBox = document.getElementById('camera-viewfinder-box');
    const videoStream = document.getElementById('camera-stream');
    const btnCapture = document.getElementById('btn-capture-photo');
    const btnCancelCamera = document.getElementById('btn-cancel-camera');
    const previewContainer = document.getElementById('image-preview-container');
    const previewImg = document.getElementById('image-preview');
    const previewInfo = document.getElementById('image-preview-info');
    const btnRemovePhoto = document.getElementById('btn-remove-photo');

    let activeStream = null;

    // Helper: Display image preview
    function displayPreview(file) {
        if (!file) return;

        // Validation (5MB max)
        if (file.size > 5 * 1024 * 1024) {
            alert('Selected image exceeds the 5MB size limit.');
            clearSelection();
            return;
        }

        const reader = new FileReader();
        reader.onload = function (e) {
            if (previewImg) previewImg.src = e.target.result;
            if (previewInfo) {
                const sizeKb = Math.round(file.size / 1024);
                previewInfo.textContent = `${file.name || 'Captured Photo'} (${sizeKb} KB)`;
            }
            if (previewContainer) previewContainer.style.display = 'block';
        };
        reader.readAsDataURL(file);
    }

    // Helper: Clear selection
    function clearSelection() {
        if (imageInput) imageInput.value = '';
        if (cameraDirectInput) cameraDirectInput.value = '';
        if (previewContainer) previewContainer.style.display = 'none';
        if (previewImg) previewImg.src = '#';
    }

    // Stop active camera stream
    function stopCamera() {
        if (activeStream) {
            activeStream.getTracks().forEach(track => track.stop());
            activeStream = null;
        }
        if (cameraBox) cameraBox.style.display = 'none';
    }

    // Button: Choose from gallery/files
    if (btnChooseFile && imageInput) {
        btnChooseFile.addEventListener('click', () => {
            stopCamera();
            imageInput.click();
        });
    }

    // File input change
    if (imageInput) {
        imageInput.addEventListener('change', function () {
            if (this.files && this.files[0]) {
                displayPreview(this.files[0]);
            }
        });
    }

    // Direct mobile camera input change (fallback)
    if (cameraDirectInput && imageInput) {
        cameraDirectInput.addEventListener('change', function () {
            if (this.files && this.files[0]) {
                const file = this.files[0];
                // Transfer file to standard imageInput
                const dt = new DataTransfer();
                dt.items.add(file);
                imageInput.files = dt.files;
                displayPreview(file);
            }
        });
    }

    // Button: Open Camera
    if (btnOpenCamera) {
        btnOpenCamera.addEventListener('click', async () => {
            // Check if mediaDevices is supported
            if (navigator.mediaDevices && navigator.mediaDevices.getUserMedia) {
                try {
                    stopCamera();
                    // Prefer back/environment camera on phones, fallback to default on laptop/desktop
                    const constraints = {
                        video: {
                            facingMode: { ideal: 'environment' },
                            width: { ideal: 1280 },
                            height: { ideal: 720 }
                        },
                        audio: false
                    };
                    activeStream = await navigator.mediaDevices.getUserMedia(constraints);
                    if (videoStream) {
                        videoStream.srcObject = activeStream;
                        videoStream.play();
                    }
                    if (cameraBox) {
                        cameraBox.style.display = 'block';
                        cameraBox.scrollIntoView({ behavior: 'smooth', block: 'nearest' });
                    }
                } catch (err) {
                    console.warn('getUserMedia failed, falling back to native file capture', err);
                    if (cameraDirectInput) {
                        cameraDirectInput.click();
                    } else if (imageInput) {
                        imageInput.click();
                    }
                }
            } else if (cameraDirectInput) {
                // Mobile browser fallback
                cameraDirectInput.click();
            } else if (imageInput) {
                imageInput.click();
            }
        });
    }

    // Button: Capture Photo from live stream
    if (btnCapture && videoStream && imageInput) {
        btnCapture.addEventListener('click', () => {
            if (!videoStream.videoWidth || !videoStream.videoHeight) {
                alert('Camera stream is not ready yet. Please wait a moment.');
                return;
            }

            const canvas = document.createElement('canvas');
            canvas.width = videoStream.videoWidth;
            canvas.height = videoStream.videoHeight;
            const ctx = canvas.getContext('2d');
            ctx.drawImage(videoStream, 0, 0, canvas.width, canvas.height);

            canvas.toBlob(blob => {
                if (blob) {
                    const fileName = `complaint_photo_${Date.now()}.jpg`;
                    const file = new File([blob], fileName, { type: 'image/jpeg' });

                    // Assign to the form's file input via DataTransfer
                    const dt = new DataTransfer();
                    dt.items.add(file);
                    imageInput.files = dt.files;

                    displayPreview(file);
                    stopCamera();
                }
            }, 'image/jpeg', 0.88);
        });
    }

    // Button: Cancel Camera
    if (btnCancelCamera) {
        btnCancelCamera.addEventListener('click', () => {
            stopCamera();
        });
    }

    // Button: Remove Photo
    if (btnRemovePhoto) {
        btnRemovePhoto.addEventListener('click', () => {
            clearSelection();
            stopCamera();
        });
    }
});
