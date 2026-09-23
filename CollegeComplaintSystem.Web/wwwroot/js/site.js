// PWA Service Worker Registration
if ('serviceWorker' in navigator) {
    window.addEventListener('load', () => {
        navigator.serviceWorker.register('/service-worker.js')
            .then(reg => console.log('CCMS Service Worker registered', reg))
            .catch(err => console.log('Service Worker registration failed', err));
    });
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
document.addEventListener('DOMContentLoaded', updateOnlineStatus);

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
