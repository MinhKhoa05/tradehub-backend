using TradeHub.DAL.Entities;

namespace TradeHub.DAL.Repositories.Interfaces
{
    public interface IWalletRepository
    {
        Task<long> CreateAsync(Wallet wallet);
        Task<Wallet?> GetByUserIdAsync(long userId);
        Task<int> IncreaseBalanceAsync(long userId, int amount);
        Task<int> DecreaseBalanceAsync(long userId, int amount);
    }
}
