using GameTopUp.DAL.Entities;

namespace GameTopUp.DAL.Interfaces
{
    public interface IWalletRepository
    {
        Task<Wallet?> GetByUserIdAsync(long userId);
        Task<Wallet?> GetByUserIdForUpdateAsync(long userId);
        Task<long> CreateAsync(Wallet wallet);
        Task<int> UpdateAsync(Wallet wallet);
        Task<int> IncreaseBalanceAsync(long userId, decimal amount);
        Task<int> DecreaseBalanceAsync(long userId, decimal amount);
    }
}
