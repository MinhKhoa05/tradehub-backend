using TradeHub.DAL.Entities;

namespace TradeHub.DAL.Repositories.Interfaces
{
    public interface IWalletTransactionRepository
    {
        Task<List<WalletTransaction>> GetByUserIdAsync(long userId);
        Task<long> CreateAsync(WalletTransaction walletTransaction);
    }
}
