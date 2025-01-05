using Microsoft.Extensions.Logging;
using YTM.Core.Entities;
using YTM.Core.Repositories;
using YTM.Core.Services;

namespace YTM.Service.Services
{
    public class CartService : ICartService
    {
        private readonly ICartRepository _cartRepository;
        private readonly IProductService _productService;
        private readonly ILogger<CartService> _logger;

        public CartService(
            ICartRepository cartRepository,
            IProductService productService,
            ILogger<CartService> logger)
        {
            _cartRepository = cartRepository;
            _productService = productService;
            _logger = logger;
        }

        public async Task<Cart> GetCartAsync(string userId)
        {
            return await _cartRepository.GetCartAsync(userId);
        }

        public async Task AddToCartAsync(string userId, string productId, int size, int quantity)
        {
            try
            {
                var product = await _productService.GetProductByIdAsync(productId);
                if (product == null)
                {
                    throw new Exception("Ürün bulunamadı");
                }

                var selectedSize = product.Sizes.FirstOrDefault(s => s.Size == size);
                if (selectedSize == null)
                {
                    throw new Exception("Geçersiz numara seçimi");
                }

                if (selectedSize.Stock < quantity)
                {
                    throw new Exception($"Seçilen numara için yetersiz stok. Mevcut stok: {selectedSize.Stock}");
                }

                var cart = await _cartRepository.GetCartAsync(userId) ?? new Cart 
                { 
                    UserId = userId,
                    Items = new List<CartItem>()
                };

                var existingItem = cart.Items.FirstOrDefault(i => i.ProductId == productId && i.Size == size);
                if (existingItem != null)
                {
                    existingItem.Quantity += quantity;
                }
                else
                {
                    cart.Items.Add(new CartItem
                    {
                        ProductId = productId,
                        ProductName = product.Name,
                        ImageUrl = product.ImageUrl,
                        Size = size,
                        Quantity = quantity,
                        Price = product.Price
                    });
                }

                await _cartRepository.UpdateCartAsync(cart);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error adding to cart: {ex.Message}");
                throw;
            }
        }

        public async Task RemoveFromCartAsync(string userId, string productId)
        {
            var cart = await _cartRepository.GetCartAsync(userId);
            if (cart != null)
            {
                cart.Items.RemoveAll(i => i.ProductId == productId);
                await _cartRepository.UpdateCartAsync(cart);
            }
        }

        public async Task ClearCartAsync(string userId)
        {
            try
            {
                var cart = await _cartRepository.GetCartAsync(userId);
                if (cart != null)
                {
                    cart.Items.Clear();
                    await _cartRepository.UpdateCartAsync(cart);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error clearing cart: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> UpdateCartItemAsync(string userId, string productId, int size, int quantity)
        {
            try
            {
                var cart = await _cartRepository.GetCartAsync(userId);
                if (cart == null)
                {
                    return false;
                }

                var cartItem = cart.Items.FirstOrDefault(i => i.ProductId == productId && i.Size == size);
                if (cartItem == null)
                {
                    return false;
                }

                // Stok kontrolü
                var product = await _productService.GetProductByIdAsync(productId);
                var sizeInfo = product?.Sizes.FirstOrDefault(s => s.Size == size);
                if (sizeInfo == null || sizeInfo.Stock < quantity)
                {
                    throw new InvalidOperationException("Yetersiz stok");
                }

                // Miktarı güncelle
                cartItem.Quantity = quantity;

                // Sepeti güncelle
                await _cartRepository.UpdateCartAsync(cart);
                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
} 