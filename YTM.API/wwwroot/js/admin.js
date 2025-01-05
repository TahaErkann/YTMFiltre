let productModal;
let deleteModal;
let selectedProductId;

document.addEventListener('DOMContentLoaded', async function() {
    try {
        await checkAuth();
        
        // Bootstrap modallarını başlat
        const addProductModalEl = document.getElementById('addProductModal');
        if (addProductModalEl) {
            productModal = new bootstrap.Modal(addProductModalEl, {
                keyboard: false,
                backdrop: 'static'
            });
        }
        
        const deleteModalEl = document.getElementById('deleteModal');
        if (deleteModalEl) {
            deleteModal = new bootstrap.Modal(deleteModalEl);
        }
        
        // Ürünleri yükle
        await loadProducts();

        // Event listener'ları ekle
        document.getElementById('addProductBtn')?.addEventListener('click', () => {
            if (productModal) productModal.show();
        });
        
        document.getElementById('addProductForm')?.addEventListener('submit', function(e) {
            e.preventDefault();
            saveProduct();
        });

    } catch (error) {
        console.error('Initialization error:', error);
        showAlert('Sayfa yüklenirken bir hata oluştu: ' + error.message, 'error');
    }
});

async function loadProducts() {
    try {
        const token = localStorage.getItem('token');
        const response = await fetch('/api/products', {
            headers: {
                'Authorization': `Bearer ${token}`
            }
        });

        if (!response.ok) {
            throw new Error(`HTTP error! status: ${response.status}`);
        }

        const products = await response.json();
        displayProducts(products);
    } catch (error) {
        console.error('Error loading products:', error);
        alert('Ürünler yüklenirken bir hata oluştu: ' + error.message);
    }
}

function displayProducts(products) {
    const tableBody = document.getElementById('productsTable');
    tableBody.innerHTML = '';

    products.forEach(product => {
        const sizeRange = product.sizes
            .map(s => s.size)
            .sort((a, b) => a - b)
            .reduce((acc, size) => {
                if (!acc.start) acc.start = size;
                acc.end = size;
                return acc;
            }, {});

        const row = document.createElement('tr');
        row.innerHTML = `
            <td>
                <img src="${product.imageUrl || '/images/no-image.png'}" 
                     class="product-image" 
                     alt="${product.name}">
            </td>
            <td>${product.name}</td>
            <td>${product.description || '-'}</td>
            <td>${product.price.toFixed(2)} ₺</td>
            <td>${product.brand || '-'}</td>
            <td>${product.category}</td>
            <td>${sizeRange.start}-${sizeRange.end}</td>
            <td>
                <span class="badge ${product.isActive ? 'bg-success' : 'bg-danger'}">
                    ${product.isActive ? 'Aktif' : 'Pasif'}
                </span>
            </td>
            <td>
                <button class="btn btn-sm btn-primary" onclick="showEditProductModal('${product.id}')">
                    <i class="bi bi-pencil"></i>
                </button>
                <button class="btn btn-sm btn-danger" onclick="showDeleteModal('${product.id}')">
                    <i class="bi bi-trash"></i>
                </button>
            </td>
        `;
        tableBody.appendChild(row);
    });
}

async function showEditProductModal(id) {
    try {
        const token = localStorage.getItem('token');
        const response = await fetch(`/api/products/${id}`, {
            headers: {
                'Authorization': `Bearer ${token}`
            }
        });

        if (!response.ok) {
            throw new Error(`HTTP error! status: ${response.status}`);
        }

        const product = await response.json();
        
        document.getElementById('modalTitle').textContent = 'Ürün Düzenle';
        document.getElementById('productId').value = product.id;
        document.getElementById('name').value = product.name;
        document.getElementById('description').value = product.description || '';
        document.getElementById('price').value = product.price;
        document.getElementById('brand').value = product.brand || '';
        document.getElementById('imageUrl').value = product.imageUrl || '';
        document.getElementById('category').value = product.category;
        document.getElementById('isActive').checked = product.isActive;

        // Numara ve stok bilgilerini doldur
        product.sizes.forEach(size => {
            const checkbox = document.getElementById(`size-${size.size}`);
            const stockInput = document.getElementById(`stock-${size.size}`);
            if (checkbox && stockInput) {
                checkbox.checked = true;
                stockInput.disabled = false;
                stockInput.value = size.stock;
            }
        });

        productModal.show();
    } catch (error) {
        console.error('Error loading product:', error);
        showAlert('Ürün bilgileri yüklenirken bir hata oluştu: ' + error.message, 'error');
    }
}

async function saveProduct() {
    try {
        const form = document.getElementById('addProductForm');
        if (!form.checkValidity()) {
            form.reportValidity();
            return;
        }

        const sizes = [];
        const sizeRows = document.querySelectorAll('.size-stock-row');
        
        sizeRows.forEach(row => {
            const size = parseInt(row.querySelector('[name="size[]"]').value);
            const stock = parseInt(row.querySelector('[name="stock[]"]').value);
            if (size && stock) {
                sizes.push({ size, stock });
            }
        });

        if (sizes.length === 0) {
            showToast('En az bir beden ve stok girmelisiniz', 'warning');
            return;
        }

        const product = {
            name: document.getElementById('productName').value,
            description: document.getElementById('productDescription').value,
            price: parseFloat(document.getElementById('productPrice').value),
            brand: document.getElementById('productBrand').value,
            imageUrl: document.getElementById('productImageUrl').value,
            category: document.getElementById('productCategory').value,
            isActive: document.getElementById('productIsActive').checked,
            sizes: sizes
        };

        // Validasyonlar
        if (!product.name) {
            showToast('Ürün adı zorunludur', 'warning');
            return;
        }

        if (!product.price || product.price <= 0) {
            showToast('Geçerli bir fiyat giriniz', 'warning');
            return;
        }

        if (!product.category) {
            showToast('Kategori seçimi zorunludur', 'warning');
            return;
        }

        const response = await fetch('/api/products', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'Authorization': `Bearer ${localStorage.getItem('token')}`
            },
            body: JSON.stringify(product)
        });

        if (!response.ok) {
            const errorData = await response.json();
            throw new Error(errorData.message || 'Ürün eklenirken bir hata oluştu');
        }

        // Modal'ı kapat ve ürünleri yeniden yükle
        const modal = bootstrap.Modal.getInstance(document.getElementById('addProductModal'));
        modal.hide();
        
        // Formu temizle
        form.reset();
        document.getElementById('sizeStockContainer').innerHTML = `
            <div class="size-stock-row d-flex gap-2 mb-2">
                <input type="number" class="form-control" placeholder="Beden" name="size[]">
                <input type="number" class="form-control" placeholder="Stok" name="stock[]">
                <button type="button" class="btn btn-danger remove-size">Sil</button>
            </div>
        `;
        
        await loadProducts();
        showToast('Ürün başarıyla eklendi', 'success');
    } catch (error) {
        console.error('Error:', error);
        showToast(error.message, 'danger');
    }
}

function showDeleteModal(id) {
    selectedProductId = id;
    deleteModal.show();
}

async function confirmDelete() {
    try {
        const token = localStorage.getItem('token');
        const response = await fetch(`/api/products/${selectedProductId}`, {
            method: 'DELETE',
            headers: {
                'Authorization': `Bearer ${token}`
            }
        });

        if (!response.ok) {
            throw new Error(`HTTP error! status: ${response.status}`);
        }

        deleteModal.hide();
        await loadProducts();
        alert('Ürün başarıyla silindi.');
    } catch (error) {
        console.error('Error deleting product:', error);
        alert('Ürün silinirken bir hata oluştu: ' + error.message);
    }
}

function checkAuth() {
    const token = localStorage.getItem('token');
    const userRole = localStorage.getItem('userRole');

    if (!token || !userRole) {
        window.location.replace('/login.html');
        return;
    }

    if (userRole !== 'Admin') {
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

// Numara seçeneklerini oluşturan fonksiyon
function initializeSizesForm() {
    const sizesGrid = document.getElementById('sizes-grid');
    if (!sizesGrid) {
        console.error('sizes-grid elementi bulunamadı');
        return;
    }

    sizesGrid.innerHTML = '';

    // Numaraları 3'lü grid şeklinde göster
    for (let size = 30; size <= 45; size++) {
        const sizeDiv = document.createElement('div');
        sizeDiv.className = 'col-4 mb-3';
        sizeDiv.innerHTML = `
            <div class="size-box">
                <div class="d-flex justify-content-between align-items-center mb-2">
                    <div class="form-check">
                        <input type="checkbox" class="form-check-input size-checkbox" 
                               id="size-${size}" data-size="${size}">
                        <label class="form-check-label fw-bold" for="size-${size}">
                            ${size} Numara
                        </label>
                    </div>
                </div>
                <div class="stock-input">
                    <div class="input-group input-group-sm">
                        <span class="input-group-text bg-light">Stok</span>
                        <input type="number" class="form-control size-stock" 
                               id="stock-${size}" placeholder="0" min="0" disabled>
                    </div>
                </div>
            </div>
        `;
        sizesGrid.appendChild(sizeDiv);
    }

    // Checkbox event listener'ları
    document.querySelectorAll('.size-checkbox').forEach(checkbox => {
        checkbox.addEventListener('change', function() {
            const stockInput = document.getElementById(`stock-${this.dataset.size}`);
            const sizeBox = this.closest('.size-box');
            
            if (stockInput) {
                stockInput.disabled = !this.checked;
                if (!this.checked) {
                    stockInput.value = '';
                    sizeBox.classList.remove('active');
                } else {
                    sizeBox.classList.add('active');
                    stockInput.focus();
                }
            }
        });
    });
}

// Alert gösterme fonksiyonu
function showAlert(message, type = 'info') {
    const alertDiv = document.createElement('div');
    alertDiv.className = `alert alert-${type} alert-dismissible fade show`;
    alertDiv.role = 'alert';
    alertDiv.innerHTML = `
        ${message}
        <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
    `;
    
    document.querySelector('main').insertAdjacentElement('afterbegin', alertDiv);
    
    // 3 saniye sonra alert'i kaldır
    setTimeout(() => {
        alertDiv.remove();
    }, 3000);
}

// Modal event listener'larını ekle
document.addEventListener('DOMContentLoaded', function() {
    const modalElement = document.getElementById('productModal');
    if (modalElement) {
        modalElement.addEventListener('shown.bs.modal', function () {
            // Modal tamamen açıldığında numara formunu başlat
            initializeSizesForm();
        });
    }
});

// Beden ekleme fonksiyonu
document.getElementById('addSizeBtn').addEventListener('click', function() {
    const container = document.getElementById('sizeStockContainer');
    const newRow = document.createElement('div');
    newRow.className = 'size-stock-row d-flex gap-2 mb-2';
    newRow.innerHTML = `
        <input type="number" class="form-control" placeholder="Beden" name="size[]">
        <input type="number" class="form-control" placeholder="Stok" name="stock[]">
        <button type="button" class="btn btn-danger remove-size">Sil</button>
    `;
    container.appendChild(newRow);
});

// Beden silme işlemi için event delegation
document.getElementById('sizeStockContainer').addEventListener('click', function(e) {
    if (e.target.classList.contains('remove-size')) {
        e.target.closest('.size-stock-row').remove();
    }
});

// Toast fonksiyonunu ekleyelim
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