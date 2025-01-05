using YTM.Core.Entities;

namespace YTM.Core.Repositories
{
    public interface ICartRepository
    {
        Task<Cart> GetCartAsync(string userId);
        Task<Cart> AddToCartAsync(Cart cart);
        Task<Cart> UpdateCartAsync(Cart cart);
        Task DeleteCartAsync(string userId);
    }
} 