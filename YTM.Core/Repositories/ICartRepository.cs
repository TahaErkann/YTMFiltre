using YTM.Core.Entities;

namespace YTM.Core.Repositories
{
    public interface ICartRepository
    {
        Task<Cart?> GetCartByUserIdAsync(string userId);
        Task AddToCartAsync(string userId, string productId, int size, int quantity);
        Task RemoveItemFromCartAsync(string userId, string productId);
        Task UpdateItemQuantityAsync(string userId, string productId, int quantity);
        Task ClearCartAsync(string userId);
    }
} 