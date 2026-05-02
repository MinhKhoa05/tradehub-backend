using GameTopUp.DAL.Entities;
using GameTopUp.DAL.Interfaces;

namespace GameTopUp.DAL.Repositories
{
    /// <summary>
    /// Repository quản lý thông tin các trò chơi hỗ trợ nạp tiền.
    /// </summary>
    public class GameRepository : IGameRepository
    {
        private readonly DatabaseContext _database;

        public GameRepository(DatabaseContext database)
        {
            _database = database;
        }

        public async Task<Game?> GetByIdAsync(long id)
        {
            var sql = "SELECT * FROM games WHERE id = @Id";
            
            return await _database.QueryFirstAsync<Game>(sql, new 
            { 
                Id = id 
            });
        }

        public async Task<List<Game>> GetAllAsync()
        {
            var sql = "SELECT * FROM games";
            
            return await _database.QueryAsync<Game>(sql);
        }

        public async Task<long> CreateAsync(Game game)
        {
            return await _database.InsertAsync<Game, long>(game);
        }

        public async Task<int> UpdateAsync(Game game)
        {
            var sql = @"UPDATE games 
                        SET name = @Name, image_url = @ImageUrl, is_active = @IsActive 
                        WHERE id = @Id";
            
            return await _database.ExecuteAsync(sql, game);
        }

        public async Task<int> DeleteAsync(long id)
        {
            var sql = "DELETE FROM games WHERE id = @Id";
            
            return await _database.ExecuteAsync(sql, new 
            { 
                Id = id 
            });
        }
    }
}
