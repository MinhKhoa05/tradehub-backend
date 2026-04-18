using TradeHub.DAL.Entities;

namespace TradeHub.DAL.Repositories.Interfaces
{
    public interface IWalletTransactionRepository
    {
        Task<List<WalletTransaction>> GetByWalletIdAsync(long walletId);
        Task<long> CreateAsync(WalletTransaction walletTransaction);
    }
}
