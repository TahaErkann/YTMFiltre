document.addEventListener('DOMContentLoaded', () => {
    initializeSizeOptions();
    loadProducts();
    setupEventListeners();
});

function initializeSizeOptions() {
    const sizeFilter = document.getElementById('sizeFilter');
    for(let size = 35; size <= 45; size++) {
        const option = document.createElement('option');
        option.value = size;
        option.textContent = size;
        sizeFilter.appendChild(option);
    }
}

function setupEventListeners() {
    document.getElementById('applyFilters').addEventListener('click', applyFilters);
    document.getElementById('resetFilters').addEventListener('click', resetFilters);
}

async function loadProducts() {
    try {
        const response = await fetch('/api/products');
        if (!response.ok) throw new Error('Ürünler yüklenemedi');
        
        const products = await response.json();
        displayProducts(products);
    } catch (error) {
        console.error('Ürünleri yükleme hatası:', error);
        alert('Ürünler yüklenirken bir hata oluştu');
    }
}

async function applyFilters() {
    const filters = {
        minPrice: document.getElementById('minPrice').value || null,
        maxPrice: document.getElementById('maxPrice').value || null,
        brand: document.getElementById('brandFilter').value || null,
        category: document.getElementById('categoryFilter').value || null,
        size: document.getElementById('sizeFilter').value || null,
        inStock: document.getElementById('inStockFilter').checked || null
    };

    try {
        const queryParams = new URLSearchParams();
        Object.entries(filters).forEach(([key, value]) => {
            if (value !== null && value !== '') {
                queryParams.append(key, value);
            }
        });

        const response = await fetch(`/api/products/filter?${queryParams}`);
        if (!response.ok) throw new Error('Filtreleme işlemi başarısız oldu');

        const products = await response.json();
        displayProducts(products);
    } catch (error) {
        console.error('Filtreleme hatası:', error);
        alert('Filtreleme sırasında bir hata oluştu');
    }
}

function displayProducts(products) {
    const container = document.getElementById('productsContainer');
    
    if (!products.length) {
        container.innerHTML = '<div class="no-products">Ürün bulunamadı</div>';
        return;
    }

    container.innerHTML = products.map(product => `
        <div class="product-card">
            <img src="${product.imageUrl || '/images/default-product.jpg'}" 
                 alt="${product.name}" 
                 class="product-image">
            <h3>${product.name}</h3>
            <p class="brand">${product.brand}</p>
            <p class="price">${product.price} TL</p>
            <div class="sizes">
                ${product.sizes.map(size => `
                    <span class="size-badge ${size.stock > 0 ? 'in-stock' : 'out-of-stock'}">
                        ${size.size}
                    </span>
                `).join('')}
            </div>
            <button onclick="addToCart('${product._id}')" 
                    class="btn primary"
                    ${!product.sizes.some(s => s.stock > 0) ? 'disabled' : ''}>
                ${product.sizes.some(s => s.stock > 0) ? 'Sepete Ekle' : 'Stokta Yok'}
            </button>
        </div>
    `).join('');
}

function resetFilters() {
    document.getElementById('minPrice').value = '';
    document.getElementById('maxPrice').value = '';
    document.getElementById('brandFilter').value = '';
    document.getElementById('categoryFilter').value = '';
    document.getElementById('sizeFilter').value = '';
    document.getElementById('inStockFilter').checked = false;
    loadProducts();
}

async function addToCart(productId) {
    // Sepete ekleme mantığı burada implement edilecek
} 