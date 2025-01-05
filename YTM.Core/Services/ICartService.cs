using YTM.Core.Entities;

namespace YTM.Core.Services
{
    public interface ICartService
    {
        Task<Cart?> GetCartAsync(string userId);
        Task AddToCartAsync(string userId, string productId, int size, int quantity);
        Task RemoveFromCartAsync(string userId, string productId);
        Task UpdateQuantityAsync(string userId, string productId, int quantity);
        Task ClearCartAsync(string userId);
    }
} 