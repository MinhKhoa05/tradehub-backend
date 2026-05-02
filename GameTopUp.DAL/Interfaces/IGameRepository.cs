using GameTopUp.DAL.Entities;

namespace GameTopUp.DAL.Interfaces
{
    public interface IGameRepository
    {
        Task<Game?> GetByIdAsync(long id);
        Task<List<Game>> GetAllAsync();
        Task<long> CreateAsync(Game game);
        Task<int> UpdateAsync(Game game);
        Task<int> DeleteAsync(long id);
    }
}
