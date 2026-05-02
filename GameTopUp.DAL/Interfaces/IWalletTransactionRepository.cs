using GameTopUp.DAL.Entities;

namespace GameTopUp.DAL.Interfaces
{
    public interface IWalletTransactionRepository
    {
        Task<List<WalletTransaction>> GetByUserIdAsync(long userId);
        Task<long> CreateAsync(WalletTransaction walletTransaction);
    }
}
