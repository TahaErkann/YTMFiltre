using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using YTM.Core.Entities;
using YTM.Core.Repositories;
using YTM.Core.Settings;

namespace YTM.Data.Repositories
{
    public class CartRepository : ICartRepository
    {
        private readonly IMongoCollection<Cart> _carts;
        private readonly ILogger<CartRepository> _logger;
        private readonly IMongoCollection<Product> _productCollection;

        public CartRepository(IOptions<DatabaseSettings> settings, ILogger<CartRepository> logger)
        {
            _logger = logger;
            var client = new MongoClient(settings.Value.ConnectionString);
            var database = client.GetDatabase(settings.Value.DatabaseName);
            _carts = database.GetCollection<Cart>("Carts");
            _productCollection = database.GetCollection<Product>("Products");
        }

        public async Task<Cart?> GetCartByUserIdAsync(string userId)
        {
            try
            {
                return await _carts.Find(x => x.UserId == userId).FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting cart: {ex.Message}");
                throw;
            }
        }

        public async Task CreateCartAsync(Cart cart)
        {
            try
            {
                await _carts.InsertOneAsync(cart);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error creating cart: {ex.Message}");
                throw;
            }
        }

        public async Task UpdateCartAsync(string userId, Cart cart)
        {
            try
            {
                cart.UpdatedAt = DateTime.UtcNow;
                await _carts.ReplaceOneAsync(x => x.UserId == userId, cart);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error updating cart: {ex.Message}");
                throw;
            }
        }

        public async Task DeleteCartAsync(string userId)
        {
            try
            {
                await _carts.DeleteOneAsync(x => x.UserId == userId);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error deleting cart: {ex.Message}");
                throw;
            }
        }

        public async Task AddItemToCartAsync(string userId, CartItem item)
        {
            try
            {
                var cart = await GetCartByUserIdAsync(userId);
                if (cart == null)
                {
                    cart = new Cart { UserId = userId, Items = new List<CartItem> { item } };
                    await CreateCartAsync(cart);
                }
                else
                {
                    var existingItem = cart.Items.FirstOrDefault(x => x.ProductId == item.ProductId);
                    if (existingItem != null)
                    {
                        existingItem.Quantity += item.Quantity;
                    }
                    else
                    {
                        cart.Items.Add(item);
                    }
                    await UpdateCartAsync(userId, cart);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error adding item to cart: {ex.Message}");
                throw;
            }
        }

        public async Task RemoveItemFromCartAsync(string userId, string productId)
        {
            try
            {
                var cart = await GetCartByUserIdAsync(userId);
                if (cart != null)
                {
                    cart.Items.RemoveAll(x => x.ProductId == productId);
                    await UpdateCartAsync(userId, cart);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error removing item from cart: {ex.Message}");
                throw;
            }
        }

        public async Task UpdateItemQuantityAsync(string userId, string productId, int quantity)
        {
            try
            {
                var cart = await GetCartByUserIdAsync(userId);
                if (cart != null)
                {
                    var item = cart.Items.FirstOrDefault(x => x.ProductId == productId);
                    if (item != null)
                    {
                        item.Quantity = quantity;
                        await UpdateCartAsync(userId, cart);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error updating item quantity: {ex.Message}");
                throw;
            }
        }

        public async Task ClearCartAsync(string userId)
        {
            try
            {
                var cart = await GetCartByUserIdAsync(userId);
                if (cart != null)
                {
                    cart.Items.Clear();
                    await UpdateCartAsync(userId, cart);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error clearing cart: {ex.Message}");
                throw;
            }
        }

        public async Task AddToCartAsync(string userId, string productId, int size, int quantity)
        {
            try
            {
                var cart = await GetCartByUserIdAsync(userId);
                if (cart == null)
                {
                    cart = new Cart 
                    { 
                        UserId = userId,
                        Items = new List<CartItem>()
                    };
                }

                var existingItem = cart.Items.FirstOrDefault(i => i.ProductId == productId && i.Size == size);
                if (existingItem != null)
                {
                    existingItem.Quantity += quantity;
                    cart.UpdatedAt = DateTime.UtcNow;
                }
                else
                {
                    var product = await _productCollection.Find(p => p.Id == productId).FirstOrDefaultAsync();
                    if (product == null)
                        throw new Exception("Ürün bulunamadı");

                    cart.Items.Add(new CartItem
                    {
                        ProductId = productId,
                        ProductName = product.Name,
                        ImageUrl = product.ImageUrl,
                        Size = size,
                        Quantity = quantity,
                        Price = product.Price
                    });
                    cart.UpdatedAt = DateTime.UtcNow;
                }

                var filter = Builders<Cart>.Filter.Eq(c => c.UserId, userId);
                var options = new ReplaceOptions { IsUpsert = true };
                await _carts.ReplaceOneAsync(filter, cart, options);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error adding item to cart: {ex.Message}");
                throw;
            }
        }
    }
} 