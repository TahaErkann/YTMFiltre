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
        private readonly IMongoCollection<Product> _products;
        private readonly ILogger<CartRepository> _logger;

        public CartRepository(IOptions<DatabaseSettings> settings, ILogger<CartRepository> logger)
        {
            _logger = logger;
            var client = new MongoClient(settings.Value.ConnectionString);
            var database = client.GetDatabase(settings.Value.DatabaseName);
            _carts = database.GetCollection<Cart>("Carts");
            _products = database.GetCollection<Product>("Products");
        }

        public async Task<Cart> GetCartAsync(string userId)
        {
            return await _carts.Find(c => c.UserId == userId).FirstOrDefaultAsync();
        }

        public async Task<Cart> AddToCartAsync(Cart cart)
        {
            await _carts.InsertOneAsync(cart);
            return cart;
        }

        public async Task<Cart> UpdateCartAsync(Cart cart)
        {
            var filter = Builders<Cart>.Filter.Eq(c => c.UserId, cart.UserId);
            await _carts.ReplaceOneAsync(filter, cart, new ReplaceOptions { IsUpsert = true });
            return cart;
        }

        public async Task DeleteCartAsync(string userId)
        {
            await _carts.DeleteOneAsync(c => c.UserId == userId);
        }
    }
} 