using TradeHub.DAL.Entities;

using TradeHub.DAL.Repositories.Interfaces;

namespace TradeHub.DAL.Repositories
{
    public class WalletRepository : IWalletRepository
    {
        private readonly DatabaseContext _database;

        public WalletRepository(DatabaseContext database)
        {
            _database = database;
        }

        public async Task<long> CreateAsync(Wallet wallet)
        {
            return await _database.InsertAsync(wallet);
        }

        public async Task<Wallet?> GetByUserIdAsync(long userId)
        {
            var sql = "SELECT * FROM wallets WHERE user_id = @UserId";
            return await _database.SqlFirstAsync<Wallet>(sql, new { UserId = userId });
        }

        public async Task<int> IncreaseBalanceAsync(long userId, int amount)
        {
            var sql = "UPDATE wallets SET balance = balance + @Amount WHERE user_id = @UserId";
            return await _database.SqlExecuteAsync(sql, new { UserId = userId, Amount = amount });
        }

        public async Task<int> DecreaseBalanceAsync(long userId, int amount)
        {
            var sql = "UPDATE wallets SET balance = balance - @Amount WHERE user_id = @UserId AND balance >= @Amount";
            return await _database.SqlExecuteAsync(sql, new { UserId = userId, Amount = amount });
        }
    }
}
