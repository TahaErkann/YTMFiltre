class FilterComponent extends HTMLElement {
    constructor() {
        super();
        this.innerHTML = `
            <div class="filter-container">
                <h3>Filtrele</h3>
                <div class="filter-section">
                    <h4>Fiyat Aralığı</h4>
                    <div class="price-range">
                        <input type="number" id="minPrice" placeholder="Min Fiyat" min="0">
                        <input type="number" id="maxPrice" placeholder="Max Fiyat" min="0">
                    </div>
                </div>

                <div class="filter-section">
                    <h4>Marka</h4>
                    <select id="brandFilter">
                        <option value="">Tümü</option>
                        <option value="Nike">Nike</option>
                        <option value="Adidas">Adidas</option>
                        <option value="Puma">Puma</option>
                    </select>
                </div>

                <div class="filter-section">
                    <h4>Kategori</h4>
                    <select id="categoryFilter">
                        <option value="">Tümü</option>
                        <option value="Erkek">Erkek</option>
                        <option value="Kadın">Kadın</option>
                        <option value="Unisex">Unisex</option>
                        <option value="Çocuk">Çocuk</option>
                    </select>
                </div>

                <div class="filter-section">
                    <h4>Numara</h4>
                    <select id="sizeFilter">
                        <option value="">Tümü</option>
                        ${Array.from({length: 16}, (_, i) => i + 35).map(size => 
                            `<option value="${size}">${size}</option>`
                        ).join('')}
                    </select>
                </div>

                <div class="filter-section">
                    <label>
                        <input type="checkbox" id="inStockFilter">
                        Sadece Stokta Olanlar
                    </label>
                </div>

                <button id="applyFilter" class="filter-button">Filtrele</button>
                <button id="resetFilter" class="filter-button">Filtreleri Temizle</button>
            </div>
        `;

        this.addStyles();
        this.attachEvents();
    }

    addStyles() {
        const style = document.createElement('style');
        style.textContent = `
            .filter-container {
                padding: 20px;
                background: #f5f5f5;
                border-radius: 8px;
                margin-bottom: 20px;
            }

            .filter-section {
                margin-bottom: 15px;
            }

            .filter-section h4 {
                margin-bottom: 8px;
                color: #333;
            }

            .price-range {
                display: flex;
                gap: 10px;
            }

            input[type="number"],
            select {
                width: 100%;
                padding: 8px;
                border: 1px solid #ddd;
                border-radius: 4px;
            }

            .filter-button {
                width: 100%;
                padding: 10px;
                margin-top: 10px;
                background: #007bff;
                color: white;
                border: none;
                border-radius: 4px;
                cursor: pointer;
            }

            .filter-button:hover {
                background: #0056b3;
            }

            #resetFilter {
                background: #6c757d;
            }

            #resetFilter:hover {
                background: #545b62;
            }
        `;
        this.appendChild(style);
    }

    attachEvents() {
        const applyFilterBtn = this.querySelector('#applyFilter');
        const resetFilterBtn = this.querySelector('#resetFilter');

        applyFilterBtn.addEventListener('click', () => this.applyFilters());
        resetFilterBtn.addEventListener('click', () => this.resetFilters());
    }

    async applyFilters() {
        const filters = {
            minPrice: this.querySelector('#minPrice').value || null,
            maxPrice: this.querySelector('#maxPrice').value || null,
            brand: this.querySelector('#brandFilter').value || null,
            category: this.querySelector('#categoryFilter').value || null,
            size: this.querySelector('#sizeFilter').value || null,
            inStock: this.querySelector('#inStockFilter').checked || null
        };

        try {
            const queryParams = new URLSearchParams();
            Object.entries(filters).forEach(([key, value]) => {
                if (value !== null) {
                    queryParams.append(key, value);
                }
            });

            const response = await fetch(`/api/products/filter?${queryParams}`);
            if (!response.ok) throw new Error('Filtreleme işlemi başarısız oldu');

            const products = await response.json();
            this.dispatchEvent(new CustomEvent('filtersApplied', { 
                detail: { products },
                bubbles: true 
            }));
        } catch (error) {
            console.error('Filtreleme hatası:', error);
            alert('Filtreleme sırasında bir hata oluştu');
        }
    }

    resetFilters() {
        this.querySelector('#minPrice').value = '';
        this.querySelector('#maxPrice').value = '';
        this.querySelector('#brandFilter').value = '';
        this.querySelector('#categoryFilter').value = '';
        this.querySelector('#sizeFilter').value = '';
        this.querySelector('#inStockFilter').checked = false;
        this.applyFilters();
    }
}

customElements.define('filter-component', FilterComponent); 