using GameTopUp.DAL.Entities;

namespace GameTopUp.DAL.Interfaces
{
    public interface IGamePackageRepository
    {
        Task<GamePackage?> GetByIdAsync(long id);
        Task<List<GamePackage>> GetAllAsync();
        Task<List<GamePackage>> GetByGameIdAsync(long gameId);
        Task<long> CreateAsync(GamePackage gamePackage);
        Task<int> UpdateAsync(GamePackage gamePackage);
        Task<int> IncreaseStockAsync(long id, int quantity);
        Task<int> DecreaseStockAsync(long id, int quantity);
        Task<int> DeleteAsync(long id);
    }
}
