using TradeHub.DAL.Entities;

namespace TradeHub.DAL.Repositories
{
    public class WalletRepository
    {
        private readonly DatabaseContext _database;

        public WalletRepository(DatabaseContext database)
        {
            _database = database;
        }

        public async Task<int> CreateAsync(Wallet wallet)
        {
            var sql = @"INSERT INTO wallets (user_id, balance)
                        VALUES (@UserId, @Balance)";
            return await _database.ExecuteInsertAsync(sql, wallet);
        }

        public async Task<Wallet?> GetByUserIdAsync(int userId)
        {
            var sql = "SELECT * FROM wallets WHERE user_id = @UserId";
            return await _database.QuerySingleAsync<Wallet>(sql, new { UserId = userId });
        }

        public async Task<int> IncreaseBalanceAsync(int userId, int amount)
        {
            var sql = "UPDATE wallets SET balance = balance + @Amount WHERE user_id = @UserId";
            return await _database.ExecuteAsync(sql, new { UserId = userId, Amount = amount });
        }

        public async Task<int> DecreaseBalanceAsync(int userId, int amount)
        {
            var sql = "UPDATE wallets SET balance = balance - @Amount WHERE user_id = @UserId AND balance >= @Amount";
            return await _database.ExecuteAsync(sql, new { UserId = userId, Amount = amount });
        }
    }
}
