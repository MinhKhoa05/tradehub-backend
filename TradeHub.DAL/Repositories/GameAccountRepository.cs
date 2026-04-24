using TradeHub.DAL.Entities;
using TradeHub.DAL.Repositories.Interfaces;

namespace TradeHub.DAL.Repositories
{
    public class GameAccountRepository : IGameAccountRepository
    {
        private readonly DatabaseContext _database;

        public GameAccountRepository(DatabaseContext database)
        {
            _database = database;
        }

        public async Task<GameAccount?> GetByIdAsync(long id)
        {
            var sql = "SELECT * FROM game_accounts WHERE id = @Id";
            return await _database.QueryFirstAsync<GameAccount>(sql, new { Id = id });
        }

        public async Task<List<GameAccount>> GetByUserIdAsync(long userId)
        {
            var sql = "SELECT * FROM game_accounts WHERE user_id = @UserId ORDER BY is_default DESC, created_at DESC";
            return await _database.QueryAsync<GameAccount>(sql, new { UserId = userId });
        }

        public async Task<long> CreateAsync(GameAccount gameAccount)
        {
            return await _database.InsertAsync<GameAccount, long>(gameAccount);
        }

        public async Task<int> UpdateAsync(GameAccount gameAccount)
        {
            var sql = @"UPDATE game_accounts 
                        SET name = @Name, account_identifier = @AccountIdentifier, server = @Server, description = @Description, is_default = @IsDefault
                        WHERE id = @Id";
            return await _database.ExecuteAsync(sql, gameAccount);
        }

        public async Task<int> DeleteAsync(long id)
        {
            var sql = "DELETE FROM game_accounts WHERE id = @Id";
            return await _database.ExecuteAsync(sql, new { Id = id });
        }

        public async Task<int> SetDefaultAsync(long userId, long id)
        {
            // First set all user's accounts to not default, then set the specified one to default
            var sqlReset = "UPDATE game_accounts SET is_default = 0 WHERE user_id = @UserId";
            await _database.ExecuteAsync(sqlReset, new { UserId = userId });

            var sqlSet = "UPDATE game_accounts SET is_default = 1 WHERE id = @Id AND user_id = @UserId";
            return await _database.ExecuteAsync(sqlSet, new { Id = id, UserId = userId });
        }
    }
}
