using TradeHub.DAL.Entities;

namespace TradeHub.DAL.Interfaces
{
    public interface IGameAccountRepository
    {
        Task<GameAccount?> GetByIdAsync(long id);
        Task<List<GameAccount>> GetByUserIdAsync(long userId);
        Task<long> CreateAsync(GameAccount gameAccount);
        Task<int> UpdateAsync(GameAccount gameAccount);
        Task<int> DeleteAsync(long id);
        Task<int> SetDefaultAsync(long userId, long id);
    }
}
