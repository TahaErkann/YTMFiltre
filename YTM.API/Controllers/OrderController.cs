using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using YTM.Core.Services;

namespace YTM.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly ICartService _cartService;
        private readonly ILogger<OrderController> _logger;

        public OrderController(
            IOrderService orderService,
            ICartService cartService,
            ILogger<OrderController> logger)
        {
            _orderService = orderService;
            _cartService = cartService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetUserOrders()
        {
            try
            {
                var userId = User.FindFirst("UserId")?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized();
                }

                var orders = await _orderService.GetUserOrdersAsync(userId);
                return Ok(orders);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting user orders: {ex.Message}");
                return StatusCode(500, "Siparişler alınırken bir hata oluştu.");
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateOrder()
        {
            try
            {
                var userId = User.FindFirst("UserId")?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized();
                }

                var cart = await _cartService.GetCartAsync(userId);
                if (cart == null || !cart.Items.Any())
                {
                    return BadRequest("Sepetiniz boş");
                }

                var order = await _orderService.CreateOrderAsync(userId, cart);
                return Ok(order);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error creating order: {ex.Message}");
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("{orderId}")]
        public async Task<IActionResult> GetOrderById(string orderId)
        {
            try
            {
                var order = await _orderService.GetOrderByIdAsync(orderId);
                if (order == null)
                {
                    return NotFound();
                }

                // Kullanıcının kendi siparişini görüntülediğinden emin olun
                var userId = User.FindFirst("UserId")?.Value;
                if (order.UserId != userId)
                {
                    return Forbid();
                }

                return Ok(order);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting order: {ex.Message}");
                return StatusCode(500, "Sipariş detayları alınırken bir hata oluştu.");
            }
        }

        [HttpPut("{orderId}/status")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateOrderStatus(string orderId, [FromBody] string status)
        {
            try
            {
                await _orderService.UpdateOrderStatusAsync(orderId, status);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error updating order status: {ex.Message}");
                return StatusCode(500, "Sipariş durumu güncellenirken bir hata oluştu.");
            }
        }
    }
} 