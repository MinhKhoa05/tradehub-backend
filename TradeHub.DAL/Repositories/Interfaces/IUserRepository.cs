using TradeHub.DAL.Entities;

namespace TradeHub.DAL.Repositories.Interfaces
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
        Task<int> IncreaseBalanceAsync(long userId, decimal amount);
        Task<int> DecreaseBalanceAsync(long userId, decimal amount);
    }
}
