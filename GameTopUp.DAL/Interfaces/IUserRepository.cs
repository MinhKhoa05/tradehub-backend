using GameTopUp.DAL.Entities;

namespace GameTopUp.DAL.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> GetByIdAsync(long userId);
        Task<User?> GetByEmailAsync(string email);
        Task<long> CreateAsync(User user);
        Task<int> UpdatePasswordAsync(long userId, string newPasswordHash);
        Task<int> UpdateAsync(User user);
        Task<IEnumerable<User>> GetAllAsync(int page, int pageSize);
        Task<int> DeleteAsync(long userId);
    }
}
