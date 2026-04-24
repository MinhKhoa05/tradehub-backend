using TradeHub.DAL.Entities;
using TradeHub.DAL.Repositories.Interfaces;

namespace TradeHub.DAL.Repositories
{
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
            return await _database.QueryFirstAsync<Game>(sql, new { Id = id });
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
            var sql = @"UPDATE games SET name = @Name, image_url = @ImageUrl WHERE id = @Id";
            return await _database.ExecuteAsync(sql, game);
        }
    }
}
