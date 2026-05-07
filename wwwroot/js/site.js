// ==================== Cart Functions ====================

// Thêm sách vào giỏ hàng
function addToCart(bookId, quantity = 1) {
    const token = getAntiForgeryToken();
    
    fetch('/Cart/AddToCart', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/x-www-form-urlencoded',
            'RequestVerificationToken': token
        },
        body: `bookId=${bookId}&quantity=${quantity}&__RequestVerificationToken=${token}`
    })
    .then(response => response.json())
    .then(data => {
        if (data.success) {
            showToast(data.message, 'success');
            updateCartBadge();
        } else {
            if (data.message.includes('đăng nhập')) {
                window.location.href = '/Account/Login?returnUrl=' + encodeURIComponent(window.location.pathname);
            } else {
                showToast(data.message, 'error');
            }
        }
    })
    .catch(error => {
        console.error('Error:', error);
        showToast('Có lỗi xảy ra. Vui lòng thử lại.', 'error');
    });
}

// Cập nhật badge số lượng giỏ hàng
function updateCartBadge() {
    fetch('/Cart/GetCartCount')
        .then(response => response.json())
        .then(data => {
            const badge = document.getElementById('cartBadge');
            if (badge) {
                if (data.count > 0) {
                    badge.textContent = data.count > 99 ? '99+' : data.count;
                    badge.style.display = 'inline-block';
                } else {
                    badge.style.display = 'none';
                }
            }
        })
        .catch(error => {
            console.error('Error updating cart badge:', error);
        });
}

// ==================== Toast Notification ====================

function showToast(message, type = 'success') {
    const toastEl = document.getElementById('toastNotification');
    const toastTitle = document.getElementById('toastTitle');
    const toastMessage = document.getElementById('toastMessage');
    const toastIcon = document.getElementById('toastIcon');
    
    if (!toastEl) {
        alert(message);
        return;
    }
    
    // Set content
    toastMessage.textContent = message;
    
    // Set icon and title based on type
    if (type === 'success') {
        toastIcon.className = 'bi bi-check-circle text-success me-2';
        toastTitle.textContent = 'Thành công';
    } else if (type === 'error') {
        toastIcon.className = 'bi bi-exclamation-circle text-danger me-2';
        toastTitle.textContent = 'Lỗi';
    } else {
        toastIcon.className = 'bi bi-info-circle text-info me-2';
        toastTitle.textContent = 'Thông báo';
    }
    
    // Show toast
    const toast = new bootstrap.Toast(toastEl, {
        autohide: true,
        delay: 3000
    });
    toast.show();
}

// ==================== Utility Functions ====================

// Lấy Anti-Forgery Token
function getAntiForgeryToken() {
    const tokenInput = document.querySelector('input[name="__RequestVerificationToken"]');
    if (tokenInput) {
        return tokenInput.value;
    }
    
    const tokenMeta = document.querySelector('meta[name="csrf-token"]');
    if (tokenMeta) {
        return tokenMeta.content;
    }
    
    return '';
}

// Format number as VND currency
function formatCurrency(number) {
    return new Intl.NumberFormat('vi-VN').format(number) + ' ₫';
}

// Debounce function
function debounce(func, wait) {
    let timeout;
    return function executedFunction(...args) {
        const later = () => {
            clearTimeout(timeout);
            func(...args);
        };
        clearTimeout(timeout);
        timeout = setTimeout(later, wait);
    };
}

// ==================== Initialize ====================

document.addEventListener('DOMContentLoaded', function() {
    // Auto-hide alerts after 5 seconds
    const alerts = document.querySelectorAll('.alert-dismissible');
    alerts.forEach(alert => {
        setTimeout(() => {
            const bsAlert = bootstrap.Alert.getOrCreateInstance(alert);
            if (bsAlert) {
                bsAlert.close();
            }
        }, 5000);
    });
    
    // Initialize tooltips
    const tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
    tooltipTriggerList.map(function (tooltipTriggerEl) {
        return new bootstrap.Tooltip(tooltipTriggerEl);
    });
});
