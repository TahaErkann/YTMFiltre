using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using YTM.Core.Entities;
using YTM.Core.Repositories;
using YTM.Core.Settings;

namespace YTM.Data.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly IMongoCollection<User> _users;
        private readonly ILogger<UserRepository> _logger;

        public UserRepository(IOptions<DatabaseSettings> settings, ILogger<UserRepository> logger)
        {
            _logger = logger;
            var client = new MongoClient(settings.Value.ConnectionString);
            var database = client.GetDatabase(settings.Value.DatabaseName);
            _users = database.GetCollection<User>(settings.Value.UsersCollectionName);
        }

        public async Task<User?> GetUserByEmailAsync(string email)
        {
            try
            {
                _logger.LogInformation($"Getting user by email: {email}");
                return await _users.Find(x => x.Email == email).FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting user by email: {ex.Message}");
                throw;
            }
        }

        public async Task<IEnumerable<User>> GetAllAsync()
        {
            return await _users.Find(_ => true).ToListAsync();
        }

        public async Task<User?> GetByIdAsync(string id)
        {
            return await _users.Find(x => x.Id == id).FirstOrDefaultAsync();
        }

        public async Task CreateAsync(User user)
        {
            await _users.InsertOneAsync(user);
        }

        public async Task UpdateAsync(string id, User user)
        {
            await _users.ReplaceOneAsync(x => x.Id == id, user);
        }

        public async Task DeleteAsync(string id)
        {
            await _users.DeleteOneAsync(x => x.Id == id);
        }
    }
} 