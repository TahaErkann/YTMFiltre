using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using YTM.Core.Services;
using System.ComponentModel.DataAnnotations;
using YTM.Core.Entities;
using YTM.Core.Models;

namespace YTM.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class CartController : ControllerBase
    {
        private readonly ICartService _cartService;
        private readonly IProductService _productService;
        private readonly ILogger<CartController> _logger;

        public CartController(ICartService cartService, IProductService productService, ILogger<CartController> logger)
        {
            _cartService = cartService;
            _productService = productService;
            _logger = logger;
        }

        private string GetUserId()
        {
            var userId = User.FindFirst("UserId")?.Value ?? 
                         User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("User ID not found in token. Claims: " + 
                    string.Join(", ", User.Claims.Select(c => $"{c.Type}: {c.Value}")));
                throw new UnauthorizedAccessException("User ID not found in token");
            }

            _logger.LogInformation($"GetUserId called. Found UserId: {userId}");
            return userId;
        }

        [HttpGet]
        public async Task<IActionResult> GetCart()
        {
            try
            {
                var userId = GetUserId();
                var cart = await _cartService.GetCartAsync(userId);
                return Ok(cart);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting cart: {ex.Message}");
                return StatusCode(500, new { message = "Sepet bilgileri alınırken bir hata oluştu." });
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddToCart([FromBody] AddToCartRequest request)
        {
            try
            {
                var userId = GetUserId();
                _logger.LogInformation($"Adding to cart for user: {userId}");

                // Ürünü getir
                var product = await _productService.GetProductByIdAsync(request.ProductId);
                if (product == null)
                {
                    return NotFound("Ürün bulunamadı");
                }

                // Seçilen numaranın stok kontrolü
                var selectedSize = product.Sizes.FirstOrDefault(s => s.Size == request.Size);
                if (selectedSize == null)
                {
                    return BadRequest("Geçersiz numara seçimi");
                }

                if (selectedSize.Stock < request.Quantity)
                {
                    return BadRequest("Yetersiz stok");
                }

                // Sepete ekle
                await _cartService.AddToCartAsync(userId, request.ProductId, request.Size, request.Quantity);

                // Güncellenmiş sepeti döndür
                var updatedCart = await _cartService.GetCartAsync(userId);
                return Ok(updatedCart);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(new { message = "Oturum süreniz dolmuş olabilir. Lütfen tekrar giriş yapın." });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error adding to cart: {ex.Message}");
                return StatusCode(500, "Ürün sepete eklenirken bir hata oluştu");
            }
        }

        [HttpPut("items/{productId}")]
        public async Task<IActionResult> UpdateCartItem(string productId, [FromBody] UpdateCartItemRequest request)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized();
                }

                // Ürünü ve stok durumunu kontrol et
                var product = await _productService.GetProductByIdAsync(productId);
                if (product == null)
                {
                    return NotFound("Ürün bulunamadı");
                }

                // İstenen numaradaki stok durumunu kontrol et
                var sizeInfo = product.Sizes.FirstOrDefault(s => s.Size == request.Size);
                if (sizeInfo == null || sizeInfo.Stock < request.Quantity)
                {
                    return BadRequest("Yetersiz stok");
                }

                // Sepeti güncelle
                var result = await _cartService.UpdateCartItemAsync(userId, productId, request.Size, request.Quantity);
                if (!result)
                {
                    return BadRequest("Sepet güncellenemedi");
                }

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error updating quantity: {ex.Message}");
                return StatusCode(500, "Ürün miktarı güncellenirken bir hata oluştu");
            }
        }

        [HttpDelete("items/{productId}")]
        public async Task<IActionResult> RemoveFromCart(string productId)
        {
            try
            {
                var userId = GetUserId();
                await _cartService.RemoveFromCartAsync(userId, productId);
                return Ok(new { message = "Ürün sepetten kaldırıldı." });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error removing from cart: {ex.Message}");
                return StatusCode(500, new { message = "Ürün sepetten kaldırılırken bir hata oluştu." });
            }
        }

        [HttpDelete]
        public async Task<IActionResult> ClearCart()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized();
                }

                await _cartService.ClearCartAsync(userId);
                
                // Boş sepeti döndür
                var emptyCart = new Cart { UserId = userId, Items = new List<CartItem>() };
                return Ok(emptyCart);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error clearing cart: {ex.Message}");
                return StatusCode(500, new { message = "Sepet temizlenirken bir hata oluştu" });
            }
        }
    }

    public class AddToCartRequest
    {
        [Required(ErrorMessage = "ProductId zorunludur")]
        public string ProductId { get; set; } = null!;

        [Required(ErrorMessage = "Size zorunludur")]
        [Range(35, 45, ErrorMessage = "Size 35-45 arasında olmalıdır")]
        public int Size { get; set; }

        [Required(ErrorMessage = "Quantity zorunludur")]
        [Range(1, 10, ErrorMessage = "Quantity 1-10 arasında olmalıdır")]
        public int Quantity { get; set; }
    }

    public class UpdateQuantityRequest
    {
        public int Quantity { get; set; }
    }
} 