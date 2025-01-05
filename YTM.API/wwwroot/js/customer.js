let cartModal = null;
let productDetailModal = null;
let selectedProduct = null;
let availableBrands = new Set();

document.addEventListener('DOMContentLoaded', function() {
    if (!checkAuth()) return;

    // Modal elementlerini seç
    const cartModalElement = document.getElementById('cartModal');
    const productDetailModalElement = document.getElementById('productDetailModal');

    // Modal'ları başlat
    if (cartModalElement) {
        cartModal = new bootstrap.Modal(cartModalElement);
    }

    if (productDetailModalElement) {
        productDetailModal = new bootstrap.Modal(productDetailModalElement);
    }

    // İlk yüklemede ürünleri getir
    loadProducts();
    loadCart();
});

async function loadProducts() {
    try {
        // Tüm aktif ürünleri getir
        const response = await fetch('/api/products', {
            headers: {
                'Authorization': `Bearer ${localStorage.getItem('token')}`
            }
        });

        if (!response.ok) {
            throw new Error('Ürünler yüklenemedi');
        }

        const products = await response.json();
        displayProducts(products);
        updateBrandFilter(products); // Marka filtresini güncelle
    } catch (error) {
        console.error('Error loading products:', error);
        showToast('Ürünler yüklenirken bir hata oluştu', 'error');
    }
}

function displayProducts(products) {
    const container = document.getElementById('productsContainer');
    
    if (!products.length) {
        container.innerHTML = '<div class="col-12 text-center p-5"><h3>Ürün bulunamadı</h3></div>';
        return;
    }

    container.innerHTML = products.map(product => `
        <div class="col-md-4 mb-4">
            <div class="card h-100">
                <img src="${product.imageUrl}" class="card-img-top" alt="${product.name}" onclick="showProductDetail('${product.id}')">
                <div class="card-body">
                    <h5 class="card-title">${product.name}</h5>
                    <p class="card-text">
                        <strong>Marka:</strong> ${product.brand}<br>
                        <strong>Fiyat:</strong> ${product.price} TL<br>
                        <strong>Numaralar:</strong> ${product.sizes.map(s => s.size).join(', ')}
                    </p>
                    <button class="btn btn-primary" onclick="showProductDetail('${product.id}')">
                        <i class="bi bi-eye"></i> Detaylar
                    </button>
                </div>
            </div>
        </div>
    `).join('');
}

function updateBrandFilter(products) {
    const brandFilter = document.getElementById('brandFilter');
    const brands = [...new Set(products.map(p => p.brand))].filter(Boolean).sort();
    
    brandFilter.innerHTML = `
        <option value="">Tüm Markalar</option>
        ${brands.map(brand => `<option value="${brand}">${brand}</option>`).join('')}
    `;
}

async function filterProducts() {
    const brand = document.getElementById('brandFilter').value;
    const size = document.getElementById('sizeFilter').value;
    const minPrice = document.getElementById('minPrice').value;
    const maxPrice = document.getElementById('maxPrice').value;

    try {
        // Eğer hiçbir filtre seçilmediyse tüm ürünleri göster
        if (!brand && !size && !minPrice && !maxPrice) {
            await loadProducts();
            return;
        }

        // Filtre varsa filtreleme endpoint'ini kullan
        let url = '/api/products/filter?';
        const params = new URLSearchParams();
        
        if (brand) params.append('brand', brand);
        if (size) params.append('size', size);
        if (minPrice) params.append('minPrice', minPrice);
        if (maxPrice) params.append('maxPrice', maxPrice);

        const response = await fetch(url + params.toString(), {
            headers: {
                'Authorization': `Bearer ${localStorage.getItem('token')}`
            }
        });

        if (!response.ok) {
            throw new Error('Filtreleme başarısız');
        }

        const products = await response.json();
        displayProducts(products);
    } catch (error) {
        console.error('Filtreleme hatası:', error);
        showToast('Filtreleme sırasında bir hata oluştu', 'error');
    }
}

function resetFilters() {
    document.getElementById('brandFilter').value = '';
    document.getElementById('minPrice').value = '';
    document.getElementById('maxPrice').value = '';
    loadProducts();
}

async function loadCart() {
    try {
        const response = await fetch('/api/cart', {
            headers: {
                'Authorization': `Bearer ${localStorage.getItem('token')}`
            }
        });

        if (!response.ok) {
            throw new Error('Sepet yüklenemedi');
        }

        const cart = await response.json();
        updateCartDisplay(cart);
    } catch (error) {
        console.error('Error loading cart:', error);
        showToast('Sepet yüklenirken bir hata oluştu', 'error');
    }
}

function updateCartDisplay(cart) {
    const cartItems = document.getElementById('cartItems');
    const emptyCart = document.getElementById('emptyCart');
    const cartTotal = document.getElementById('cartTotal');
    const checkoutButton = document.getElementById('checkoutButton');
    const cartCount = document.getElementById('cartCount');

    // Sepet boşsa
    if (!cart || !cart.items || cart.items.length === 0) {
        if (cartItems) cartItems.innerHTML = '';
        if (emptyCart) emptyCart.style.display = 'block';
        if (checkoutButton) checkoutButton.style.display = 'none';
        if (cartTotal) cartTotal.textContent = '0.00 TL';
        if (cartCount) cartCount.textContent = '0';
        return;
    }

    // Sepet doluysa
    if (emptyCart) emptyCart.style.display = 'none';
    if (checkoutButton) checkoutButton.style.display = 'block';

    // Toplam fiyat ve ürün sayısı hesaplama
    let totalPrice = 0;
    let totalItems = 0;

    cart.items.forEach(item => {
        totalPrice += item.price * item.quantity;
        totalItems += item.quantity;
    });

    // Sepet içeriğini güncelle
    if (cartItems) {
        cartItems.innerHTML = cart.items.map(item => `
            <div class="cart-item mb-3">
                <div class="d-flex align-items-center">
                    <img src="${item.imageUrl}" alt="${item.productName}" class="cart-item-image me-3" style="width: 60px; height: 60px; object-fit: cover;">
                    <div class="flex-grow-1">
                        <h6 class="mb-0">${item.productName}</h6>
                        <p class="mb-0 text-muted">
                            Numara: ${item.size} | Adet: ${item.quantity} | 
                            Fiyat: ${item.price} TL
                        </p>
                    </div>
                    <div class="d-flex align-items-center">
                        <button class="btn btn-sm btn-outline-secondary me-2" 
                                onclick="event.stopPropagation(); updateQuantity('${item.productId}', ${item.quantity - 1}, ${item.size})">
                            <i class="bi bi-dash"></i>
                        </button>
                        <span class="mx-2">${item.quantity}</span>
                        <button class="btn btn-sm btn-outline-secondary ms-2" 
                                onclick="event.stopPropagation(); updateQuantity('${item.productId}', ${item.quantity + 1}, ${item.size})">
                            <i class="bi bi-plus"></i>
                        </button>
                        <button class="btn btn-sm btn-outline-danger ms-3" 
                                onclick="event.stopPropagation(); removeFromCart('${item.productId}')">
                            <i class="bi bi-trash"></i>
                        </button>
                    </div>
                </div>
            </div>
        `).join('');
    }

    // Toplam fiyat ve ürün sayısını güncelle
    if (cartTotal) cartTotal.textContent = `${totalPrice.toFixed(2)} TL`;
    if (cartCount) cartCount.textContent = totalItems.toString();
}

async function addToCart() {
    try {
        const sizeSelect = document.getElementById('sizeSelect');
        const quantityInput = document.getElementById('quantityInput');
        
        if (!selectedProduct || !sizeSelect.value || !quantityInput.value) {
            showToast('Lütfen numara ve adet seçin', 'warning');
            return;
        }

        const response = await fetch('/api/cart', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'Authorization': `Bearer ${localStorage.getItem('token')}`
            },
            body: JSON.stringify({
                productId: selectedProduct.id,
                size: parseInt(sizeSelect.value),
                quantity: parseInt(quantityInput.value)
            })
        });

        if (!response.ok) {
            throw new Error('Sepete eklenemedi');
        }

        await loadCart();
        productDetailModal.hide();
        showToast('Ürün sepete eklendi', 'success');
    } catch (error) {
        console.error('Error:', error);
        showToast('Ürün sepete eklenirken bir hata oluştu', 'error');
    }
}

async function addItemToCart(productId, size, quantity) {
    try {
        const token = localStorage.getItem('token');
        if (!token) {
            window.location.replace('/login.html');
            throw new Error('Oturum süreniz dolmuş. Lütfen tekrar giriş yapın.');
        }

        console.log('Token:', token); // Debug için

        const response = await fetch('/api/cart', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'Authorization': `Bearer ${token.trim()}` // token'ı temizle
            },
            body: JSON.stringify({
                productId: productId,
                size: size,
                quantity: quantity
            })
        });

        if (!response.ok) {
            if (response.status === 401) {
                localStorage.clear(); // Tüm storage'ı temizle
                window.location.replace('/login.html');
                throw new Error('Oturum süreniz dolmuş. Lütfen tekrar giriş yapın.');
            }

            const errorData = await response.json();
            throw new Error(errorData.message || 'Ürün sepete eklenirken bir hata oluştu');
        }

        await loadCart();
        showToast('Ürün sepete eklendi', 'success');
    } catch (error) {
        console.error('Error adding item to cart:', error);
        throw error;
    }
}

async function updateQuantity(productId, newQuantity, size) {
    try {
        // Eğer miktar 0 veya daha az ise ürünü sepetten kaldır
        if (newQuantity <= 0) {
            await removeFromCart(productId);
            return;
        }

        const response = await fetch(`/api/cart/items/${productId}`, {
            method: 'PUT',
            headers: {
                'Content-Type': 'application/json',
                'Authorization': `Bearer ${localStorage.getItem('token')}`
            },
            body: JSON.stringify({
                size: parseInt(size),
                quantity: parseInt(newQuantity)
            })
        });

        if (!response.ok) {
            // Hata mesajını al
            const errorData = await response.json();
            throw new Error(errorData.message || 'Miktar güncellenemedi');
        }

        await loadCart();
        showToast('Miktar güncellendi', 'success');
    } catch (error) {
        console.error('Error updating quantity:', error);
        showToast(error.message || 'Miktar güncellenirken bir hata oluştu', 'error');
    }
}

async function removeFromCart(productId) {
    try {
        const response = await fetch(`/api/cart/items/${productId}`, {
            method: 'DELETE',
            headers: {
                'Authorization': `Bearer ${localStorage.getItem('token')}`
            }
        });

        if (!response.ok) {
            throw new Error('Ürün sepetten silinemedi');
        }

        await loadCart();
        showToast('Ürün sepetten silindi', 'success');
    } catch (error) {
        console.error('Error removing item from cart:', error);
        showToast('Ürün sepetten silinirken bir hata oluştu', 'error');
    }
}

async function clearCart() {
    try {
        const response = await fetch('/api/cart', {
            method: 'DELETE',
            headers: {
                'Authorization': `Bearer ${localStorage.getItem('token')}`
            }
        });

        if (!response.ok) {
            throw new Error('Sepet temizlenemedi');
        }

        // Sepeti yeniden yükle
        await loadCart();
        
        // Toast mesajı göster
        showToast('Sepet başarıyla temizlendi', 'success');

        // Modal'ı kapat
        if (cartModal) {
            cartModal.hide();
        }
    } catch (error) {
        console.error('Error clearing cart:', error);
        showToast('Sepet temizlenirken bir hata oluştu', 'error');
    }
}

function showCart() {
    try {
        // Modal'ı kontrol et ve gerekirse yeniden başlat
        if (!cartModal) {
            const modalElement = document.getElementById('cartModal');
            if (modalElement) {
                cartModal = new bootstrap.Modal(modalElement);
            } else {
                throw new Error('Sepet modalı bulunamadı');
            }
        }
        
        // Sepeti yükle ve modalı göster
        loadCart().then(() => {
            cartModal.show();
        });
    } catch (error) {
        console.error('Error showing cart:', error);
        showToast('Sepet açılırken bir hata oluştu', 'error');
    }
}

// Toast container'ı sayfa yüklendiğinde oluştur
document.addEventListener('DOMContentLoaded', function() {
    if (!document.getElementById('toastContainer')) {
        const container = document.createElement('div');
        container.id = 'toastContainer';
        container.className = 'toast-container position-fixed bottom-0 end-0 p-3';
        document.body.appendChild(container);
    }
});

function showToast(message, type = 'success') {
    const toastId = 'toast_' + new Date().getTime();
    const bgColor = type === 'success' ? 'bg-success' : 
                    type === 'error' ? 'bg-danger' : 
                    type === 'info' ? 'bg-info' : 'bg-primary';

    const toastHtml = `
        <div id="${toastId}" class="toast ${bgColor} text-white" role="alert" aria-live="assertive" aria-atomic="true">
            <div class="toast-header ${bgColor} text-white">
                <strong class="me-auto">Bildirim</strong>
                <button type="button" class="btn-close btn-close-white" data-bs-dismiss="toast" aria-label="Close"></button>
            </div>
            <div class="toast-body">
                ${message}
            </div>
        </div>
    `;

    const container = document.getElementById('toastContainer');
    container.insertAdjacentHTML('beforeend', toastHtml);

    const toastElement = document.getElementById(toastId);
    const toast = new bootstrap.Toast(toastElement, {
        delay: 3000,
        autohide: true
    });

    toast.show();

    // Toast kapandığında DOM'dan kaldır
    toastElement.addEventListener('hidden.bs.toast', function () {
        toastElement.remove();
    });
}

async function checkout() {
    try {
        console.log('Checkout function started');

        const shippingAddress = document.getElementById('shippingAddress').value;
        const paymentMethod = document.getElementById('paymentMethod').value;

        console.log('Form values:', { shippingAddress, paymentMethod });

        if (!shippingAddress || !paymentMethod) {
            showToast('Lütfen tüm alanları doldurun', 'error');
            return;
        }

        // Sepet boş mu kontrol et
        const cartItems = document.getElementById('cartItems');
        if (!cartItems || cartItems.children.length === 0) {
            showToast('Sepetiniz boş. Sipariş oluşturulamadı!', 'error');
            return;
        }

        const token = localStorage.getItem('token');
        if (!token) {
            showToast('Oturum süreniz dolmuş. Lütfen tekrar giriş yapın.', 'error');
            window.location.href = '/login.html';
            return;
        }

        console.log('Making API request with token:', token);
        showToast('Siparişiniz işleniyor...', 'info');

        const response = await fetch('/api/order', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'Authorization': `Bearer ${token}`
            },
            body: JSON.stringify({
                shippingAddress,
                paymentMethod
            })
        });

        console.log('API Response status:', response.status);
        const responseData = await response.json();
        console.log('API Response data:', responseData);

        if (!response.ok) {
            throw new Error(responseData.message || 'Sipariş oluşturulurken bir hata oluştu');
        }

        showToast('Siparişiniz başarıyla oluşturuldu! Teşekkür ederiz.', 'success');

        // Modal'ı kapat
        const cartModal = bootstrap.Modal.getInstance(document.getElementById('cartModal'));
        cartModal.hide();

        // Formu sıfırla
        document.getElementById('shippingAddress').value = '';
        document.getElementById('paymentMethod').value = '';
        document.getElementById('orderForm').style.display = 'none';
        document.getElementById('checkoutButton').style.display = 'inline-block';
        document.getElementById('confirmOrderButton').style.display = 'none';

        await loadCart();
    } catch (error) {
        console.error('Checkout error:', error);
        showToast(error.message || 'Sipariş oluşturulurken bir hata oluştu', 'error');
    }
}

function showOrderForm() {
    console.log('Showing order form'); // Debug log
    const orderForm = document.getElementById('orderForm');
    const checkoutButton = document.getElementById('checkoutButton');
    const confirmOrderButton = document.getElementById('confirmOrderButton');

    if (!orderForm || !checkoutButton || !confirmOrderButton) {
        console.error('Required elements not found!');
        return;
    }

    orderForm.style.display = 'block';
    checkoutButton.style.display = 'none';
    confirmOrderButton.style.display = 'inline-block';
}

function checkAuth() {
    const token = localStorage.getItem('token');
    const userRole = localStorage.getItem('userRole');

    if (!token || !userRole) {
        window.location.replace('/login.html');
        return;
    }

    if (userRole !== 'Customer') {
        alert('Bu sayfaya erişim yetkiniz yok!');
        window.location.replace('/login.html');
        return;
    }
}

function logout() {
    localStorage.removeItem('token');
    localStorage.removeItem('userRole');
    window.location.replace('/login.html');
}

// Numara seçildiğinde stok kontrolü
document.getElementById('sizeSelect')?.addEventListener('change', function() {
    const selectedSize = selectedProduct?.sizes.find(s => s.size === parseInt(this.value));
    const quantityInput = document.getElementById('quantityInput');
    
    if (selectedSize) {
        quantityInput.max = selectedSize.stock;
        quantityInput.value = Math.min(quantityInput.value, selectedSize.stock);
    }
});

// Sepet sayısını güncelleme fonksiyonu
function updateCartCount(count) {
    const cartCountElement = document.getElementById('cartCount');
    if (cartCountElement) {
        cartCountElement.textContent = count;
    }
}

// Toast bildirimi gösterme fonksiyonu
function showToast(message, type = 'info') {
    const toastContainer = document.getElementById('toastContainer');
    const toast = document.createElement('div');
    toast.className = `toast align-items-center text-white bg-${type} border-0`;
    toast.setAttribute('role', 'alert');
    toast.setAttribute('aria-live', 'assertive');
    toast.setAttribute('aria-atomic', 'true');
    
    toast.innerHTML = `
        <div class="d-flex">
            <div class="toast-body">
                ${message}
            </div>
            <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast"></button>
        </div>
    `;
    
    toastContainer.appendChild(toast);
    const bsToast = new bootstrap.Toast(toast);
    bsToast.show();
    
    toast.addEventListener('hidden.bs.toast', () => {
        toast.remove();
    });
}

// Token kontrolü için yardımcı fonksiyon
function checkToken() {
    const token = localStorage.getItem('token');
    if (!token) {
        window.location.replace('/login.html');
        return false;
    }
    return true;
}

// Debounce fonksiyonu
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

async function showProductDetail(productId) {
    try {
        // Modal'ı kontrol et ve gerekirse yeniden başlat
        if (!productDetailModal) {
            const modalElement = document.getElementById('productDetailModal');
            if (modalElement) {
                productDetailModal = new bootstrap.Modal(modalElement);
            } else {
                throw new Error('Modal elementi bulunamadı');
            }
        }

        const response = await fetch(`/api/products/${productId}`, {
            headers: {
                'Authorization': `Bearer ${localStorage.getItem('token')}`
            }
        });

        if (!response.ok) {
            throw new Error('Ürün detayları alınamadı');
        }

        selectedProduct = await response.json();

        // Modal içeriğini güncelle
        const modalImg = document.getElementById('modalProductImage');
        const modalName = document.getElementById('modalProductName');
        const modalDesc = document.getElementById('modalProductDescription');
        const modalPrice = document.getElementById('modalProductPrice');
        const sizeSelect = document.getElementById('sizeSelect');

        if (modalImg) modalImg.src = selectedProduct.imageUrl;
        if (modalName) modalName.textContent = selectedProduct.name;
        if (modalDesc) modalDesc.textContent = selectedProduct.description;
        if (modalPrice) modalPrice.textContent = `${selectedProduct.price} TL`;

        // Numara seçeneklerini güncelle
        if (sizeSelect) {
            sizeSelect.innerHTML = selectedProduct.sizes
                .filter(s => s.stock > 0)
                .map(s => `<option value="${s.size}">Numara: ${s.size} - Stok: ${s.stock}</option>`)
                .join('');
        }

        // Quantity input'u sıfırla
        const quantityInput = document.getElementById('quantityInput');
        if (quantityInput) quantityInput.value = 1;

        // Modalı göster
        productDetailModal.show();

    } catch (error) {
        console.error('Error:', error);
        showToast('Ürün detayları yüklenirken bir hata oluştu', 'error');
    }
} 