using YTM.Core.Entities;

namespace YTM.Core.Services
{
    public interface IOrderService
    {
        Task<IEnumerable<Order>> GetUserOrdersAsync(string userId);
        Task<Order?> GetOrderByIdAsync(string orderId);
        Task UpdateOrderStatusAsync(string orderId, string status);
        Task<Order> CreateOrderAsync(string userId, Cart cart);
    }
} 