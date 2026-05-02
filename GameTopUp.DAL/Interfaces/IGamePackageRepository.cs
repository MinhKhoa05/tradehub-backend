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
        Task<int> UpdateStockBudgetAsync(long id, decimal spentAmount);
        Task<int> DeleteAsync(long id);
    }
}
