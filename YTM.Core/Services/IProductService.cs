using YTM.Core.Entities;

namespace YTM.Core.Services
{
    public interface IProductService
    {
        Task<IEnumerable<Product>> GetAllProductsAsync();
        Task<Product?> GetProductByIdAsync(string id);
        Task CreateProductAsync(Product product);
        Task UpdateProductAsync(string id, Product product);
        Task DeleteProductAsync(string id);
        Task UpdateStockAsync(string productId, int size, int quantity, bool increase = false);
    }
} 