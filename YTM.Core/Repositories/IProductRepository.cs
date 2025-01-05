using YTM.Core.Entities;
using MongoDB.Bson;
using MongoDB.Driver;

namespace YTM.Core.Repositories
{
    public interface IProductRepository
    {
        Task<IEnumerable<Product>> GetAllAsync();
        Task<Product?> GetByIdAsync(string id);
        Task CreateAsync(Product product);
        Task UpdateAsync(string id, Product product);
        Task DeleteAsync(string id);
        Task UpdateStockAsync(FilterDefinition<Product> filter, UpdateDefinition<Product> update);
    }
} 