using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using YTM.Core.Entities;
using YTM.Core.Repositories;
using YTM.Core.Settings;

namespace YTM.Data.Repositories
{
    public class ProductRepository : IProductRepository
    {
        private readonly IMongoCollection<Product> _products;
        private readonly ILogger<ProductRepository> _logger;

        public ProductRepository(IOptions<DatabaseSettings> dbSettings, ILogger<ProductRepository> logger)
        {
            var mongoClient = new MongoClient(dbSettings.Value.ConnectionString);
            var database = mongoClient.GetDatabase(dbSettings.Value.DatabaseName);
            _products = database.GetCollection<Product>(dbSettings.Value.ProductsCollectionName);
            _logger = logger;
        }

        public async Task<IEnumerable<Product>> GetAllAsync()
        {
            try
            {
                return await _products.Find(_ => true).ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in GetAllAsync: {ex.Message}");
                throw;
            }
        }

        public async Task<Product?> GetByIdAsync(string id)
        {
            try
            {
                return await _products.Find(x => x.Id == id).FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting product by id: {ex.Message}");
                throw;
            }
        }

        public async Task CreateAsync(Product product)
        {
            try
            {
                await _products.InsertOneAsync(product);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in CreateAsync: {ex.Message}");
                throw;
            }
        }

        public async Task UpdateAsync(string id, Product product)
        {
            try
            {
                await _products.ReplaceOneAsync(p => p.Id == id, product);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in UpdateAsync: {ex.Message}");
                throw;
            }
        }

        public async Task DeleteAsync(string id)
        {
            try
            {
                await _products.DeleteOneAsync(p => p.Id == id);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in DeleteAsync: {ex.Message}");
                throw;
            }
        }

        public async Task UpdateStockAsync(FilterDefinition<Product> filter, UpdateDefinition<Product> update)
        {
            try
            {
                _logger.LogInformation("Updating product stock");
                var result = await _products.UpdateOneAsync(filter, update);
                
                if (result.ModifiedCount == 0)
                {
                    _logger.LogWarning("No product stock was updated");
                    throw new Exception("Ürün stoku güncellenemedi");
                }

                _logger.LogInformation("Product stock updated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error updating product stock: {ex.Message}");
                throw;
            }
        }
    }
} 