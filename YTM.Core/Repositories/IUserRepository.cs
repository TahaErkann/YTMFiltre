using YTM.Core.Entities;

namespace YTM.Core.Repositories
{
    public interface IUserRepository : IGenericRepository<User>
    {
        Task<User?> GetUserByEmailAsync(string email);
    }
} 