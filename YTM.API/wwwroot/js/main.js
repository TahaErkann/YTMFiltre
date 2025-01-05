// Ana sayfa için JavaScript kodları
document.addEventListener('DOMContentLoaded', function() {
    loadFeaturedProducts();
});

async function loadFeaturedProducts() {
    try {
        const response = await fetch('/api/products/active');
        const products = await response.json();
        
        const productGrid = document.getElementById('featuredProducts');
        productGrid.innerHTML = products.map(product => `
            <div class="product-card">
                <img src="${product.imageUrl}" alt="${product.name}">
                <h3>${product.name}</h3>
                <p>${product.description}</p>
                <p class="price">${product.price} TL</p>
                <button onclick="addToCart(${product.id})">Sepete Ekle</button>
            </div>
        `).join('');
    } catch (error) {
        console.error('Ürünler yüklenirken hata oluştu:', error);
    }
} 