using System;
using System.Collections.Generic;
using System.Linq;
using YTM.Core.Entities;

namespace YTM.Core.Iterators
{
    public class FilteredProductIterator : IProductIterator
    {
        private readonly IEnumerable<Product> _filteredProducts;
        private IEnumerator<Product>? _enumerator;

        public FilteredProductIterator(IEnumerable<Product> products, string? brand = null, int? size = null, decimal? minPrice = null, decimal? maxPrice = null)
        {
            _filteredProducts = products.Where(p => 
                (string.IsNullOrEmpty(brand) || (p.Brand != null && p.Brand.ToLower().Contains(brand.ToLower()))) &&
                (!size.HasValue || p.Sizes.Any(s => s.Size == size.Value && s.Stock > 0)) &&
                (!minPrice.HasValue || p.Price >= minPrice) &&
                (!maxPrice.HasValue || p.Price <= maxPrice)
            );
            Reset();
        }

        public bool HasNext()
        {
            return _enumerator?.MoveNext() ?? false;
        }

        public Product Next()
        {
            if (_enumerator?.Current == null)
            {
                throw new InvalidOperationException("No current element");
            }
            return _enumerator.Current;
        }

        public void Reset()
        {
            _enumerator = _filteredProducts.GetEnumerator();
        }
    }
}