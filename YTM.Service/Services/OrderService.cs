using Microsoft.Extensions.Logging;
using YTM.Core.Entities;
using YTM.Core.Repositories;
using YTM.Core.Services;

namespace YTM.Service.Services
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IProductService _productService;
        private readonly ICartService _cartService;
        private readonly ILogger<OrderService> _logger;

        public OrderService(
            IOrderRepository orderRepository,
            IProductService productService,
            ICartService cartService,
            ILogger<OrderService> logger)
        {
            _orderRepository = orderRepository;
            _productService = productService;
            _cartService = cartService;
            _logger = logger;
        }

        public async Task<IEnumerable<Order>> GetUserOrdersAsync(string userId)
        {
            return await _orderRepository.GetUserOrdersAsync(userId);
        }

        public async Task<Order?> GetOrderByIdAsync(string orderId)
        {
            return await _orderRepository.GetOrderByIdAsync(orderId);
        }

        public async Task UpdateOrderStatusAsync(string orderId, string status)
        {
            await _orderRepository.UpdateOrderStatusAsync(orderId, status);
        }

        public async Task<Order> CreateOrderAsync(string userId, Cart cart)
        {
            try
            {
                var order = new Order
                {
                    UserId = userId,
                    Items = cart.Items.Select(item => new OrderItem
                    {
                        ProductId = item.ProductId,
                        ProductName = item.ProductName,
                        Size = item.Size,
                        Quantity = item.Quantity,
                        Price = item.Price
                    }).ToList(),
                    TotalAmount = cart.TotalAmount,
                    Status = OrderStatus.Pending,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                // Her ürün için stok kontrolü ve güncelleme
                foreach (var item in cart.Items)
                {
                    var product = await _productService.GetProductByIdAsync(item.ProductId);
                    if (product == null)
                        throw new Exception($"Ürün bulunamadı: {item.ProductId}");

                    var selectedSize = product.Sizes.FirstOrDefault(s => s.Size == item.Size);
                    if (selectedSize == null)
                        throw new Exception($"Geçersiz numara: {item.Size}");

                    if (selectedSize.Stock < item.Quantity)
                        throw new Exception($"Yetersiz stok. Ürün: {item.ProductName}, Numara: {item.Size}, Mevcut Stok: {selectedSize.Stock}");

                    // Stok güncelleme
                    await _productService.UpdateStockAsync(item.ProductId, item.Size, item.Quantity);
                }

                var createdOrder = await _orderRepository.CreateOrderAsync(order);
                await _cartService.ClearCartAsync(userId);

                return createdOrder;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error creating order: {ex.Message}");
                throw;
            }
        }
    }
} 