using YTM.Core.Entities;

namespace YTM.Core.Services
{
    public interface IUserService
    {
        Task<User?> GetUserByEmailAsync(string email);
        Task<bool> EmailExistsAsync(string email);
        Task CreateUserAsync(User user);
        Task<User?> GetUserByIdAsync(string id);
        Task UpdateUserAsync(string id, User user);
        Task DeleteUserAsync(string id);
    }
} 