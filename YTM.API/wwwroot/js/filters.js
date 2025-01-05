async function applyFilters() {
    const brand = document.getElementById('brandFilter').value;
    const minPrice = document.getElementById('minPrice').value;
    const maxPrice = document.getElementById('maxPrice').value;

    try {
        let url = '/api/products/filter?';
        const params = new URLSearchParams();
        
        if (brand) params.append('brand', brand);
        if (minPrice) params.append('minPrice', minPrice);
        if (maxPrice) params.append('maxPrice', maxPrice);

        const response = await fetch(url + params.toString(), {
            headers: {
                'Authorization': `Bearer ${localStorage.getItem('token')}`
            }
        });

        if (!response.ok) {
            throw new Error('Filtreleme işlemi başarısız oldu');
        }

        const products = await response.json();
        updateProductGrid(products);
    } catch (error) {
        console.error('Filtreleme hatası:', error);
        showToast('Filtreleme sırasında bir hata oluştu', 'error');
    }
}

function updateProductGrid(products) {
    const productGrid = document.getElementById('productGrid');
    productGrid.innerHTML = products.map(product => `
        <div class="product-card">
            <img src="${product.imageUrl}" alt="${product.name}">
            <h3>${product.name}</h3>
            <p class="brand">${product.brand}</p>
            <p class="price">${product.price} TL</p>
            <button onclick="showProductDetail('${product.id}')">Detaylar</button>
        </div>
    `).join('');
} 