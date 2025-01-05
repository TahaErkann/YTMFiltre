using Microsoft.Extensions.Logging;
using YTM.Core.Entities;
using YTM.Core.Repositories;
using YTM.Core.Services;

namespace YTM.Service.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly ILogger<UserService> _logger;

        public UserService(IUserRepository userRepository, ILogger<UserService> logger)
        {
            _userRepository = userRepository;
            _logger = logger;
        }

        public async Task<User?> GetUserByEmailAsync(string email)
        {
            try
            {
                _logger.LogInformation($"Getting user by email: {email}");
                return await _userRepository.GetUserByEmailAsync(email);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting user by email: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            try
            {
                _logger.LogInformation($"Checking if email exists: {email}");
                var user = await _userRepository.GetUserByEmailAsync(email);
                return user != null;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error checking email existence: {ex.Message}");
                throw;
            }
        }

        public async Task CreateUserAsync(User user)
        {
            try
            {
                _logger.LogInformation($"Creating new user with email: {user.Email}");
                await _userRepository.CreateAsync(user);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error creating user: {ex.Message}");
                throw;
            }
        }

        public async Task<User?> GetUserByIdAsync(string id)
        {
            try
            {
                _logger.LogInformation($"Getting user by id: {id}");
                return await _userRepository.GetByIdAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting user by id: {ex.Message}");
                throw;
            }
        }

        public async Task UpdateUserAsync(string id, User user)
        {
            try
            {
                _logger.LogInformation($"Updating user: {id}");
                await _userRepository.UpdateAsync(id, user);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error updating user: {ex.Message}");
                throw;
            }
        }

        public async Task DeleteUserAsync(string id)
        {
            try
            {
                _logger.LogInformation($"Deleting user: {id}");
                await _userRepository.DeleteAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error deleting user: {ex.Message}");
                throw;
            }
        }
    }
}