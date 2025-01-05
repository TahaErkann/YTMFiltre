using Microsoft.Extensions.Logging;
using YTM.Core.Entities;
using YTM.Core.Repositories;
using YTM.Core.Services;
using MongoDB.Driver;

namespace YTM.Service.Services
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;
        private readonly ILogger<ProductService> _logger;

        public ProductService(IProductRepository productRepository, ILogger<ProductService> logger)
        {
            _productRepository = productRepository;
            _logger = logger;
        }

        public async Task<IEnumerable<Product>> GetAllProductsAsync()
        {
            try
            {
                var products = await _productRepository.GetAllAsync();
                _logger.LogInformation($"Retrieved {products.Count()} products");
                return products;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting products: {ex.Message}");
                throw;
            }
        }

        public async Task<Product?> GetProductByIdAsync(string id)
        {
            try
            {
                _logger.LogInformation($"Getting product by id: {id}");
                return await _productRepository.GetByIdAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting product by id: {ex.Message}");
                throw;
            }
        }

        public async Task CreateProductAsync(Product product)
        {
            try
            {
                _logger.LogInformation($"Creating new product: {product.Name}");
                await _productRepository.CreateAsync(product);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error creating product: {ex.Message}");
                throw;
            }
        }

        public async Task UpdateProductAsync(string id, Product product)
        {
            try
            {
                _logger.LogInformation($"Updating product: {id}");
                await _productRepository.UpdateAsync(id, product);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error updating product: {ex.Message}");
                throw;
            }
        }

        public async Task DeleteProductAsync(string id)
        {
            try
            {
                _logger.LogInformation($"Deleting product: {id}");
                await _productRepository.DeleteAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error deleting product: {ex.Message}");
                throw;
            }
        }

        public async Task UpdateStockAsync(string productId, int size, int quantity, bool increase = false)
        {
            var product = await GetProductByIdAsync(productId);
            if (product == null)
                throw new Exception("Ürün bulunamadı");

            var sizeInfo = product.Sizes.FirstOrDefault(s => s.Size == size);
            if (sizeInfo == null)
                throw new Exception("Geçersiz numara");

            if (!increase && sizeInfo.Stock < quantity)
                throw new Exception("Yetersiz stok");

            // Stok güncelleme
            var filter = Builders<Product>.Filter.And(
                Builders<Product>.Filter.Eq(p => p.Id, productId),
                Builders<Product>.Filter.ElemMatch(p => p.Sizes, s => s.Size == size)
            );

            var update = increase
                ? Builders<Product>.Update.Inc("Sizes.$.Stock", quantity)
                : Builders<Product>.Update.Inc("Sizes.$.Stock", -quantity);

            await _productRepository.UpdateStockAsync(filter, update);
        }
    }
} 