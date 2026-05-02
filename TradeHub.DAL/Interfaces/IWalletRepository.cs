using TradeHub.DAL.Entities;

namespace TradeHub.DAL.Interfaces
{
    public interface IWalletRepository
    {
        Task<Wallet?> GetByUserIdAsync(long userId);
        Task<long> CreateAsync(Wallet wallet);
        Task<int> IncreaseBalanceAsync(long userId, decimal amount);
        Task<int> DecreaseBalanceAsync(long userId, decimal amount);
    }
}
