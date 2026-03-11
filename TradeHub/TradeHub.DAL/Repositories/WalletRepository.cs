using TradeHub.DAL.Entities;

namespace TradeHub.DAL.Repositories
{
    public class WalletRepository
    {
        private readonly DatabaseContext _databaseContext;

        public WalletRepository(DatabaseContext databaseContext)
        {
            _databaseContext = databaseContext;
        }

        public async Task<int> CreateAsync(Wallet wallet)
        {
            var sql = @"INSERT INTO wallets (user_id, balance)
                        VALUES (@UserId, @Balance)";
            return await _databaseContext.ExecuteInsertAsync(sql, wallet);
        }

        public async Task<Wallet?> GetByUserIdAsync(int userId)
        {
            var sql = "SELECT * FROM wallets WHERE user_id = @UserId";
            return await _databaseContext.QuerySingleAsync<Wallet>(sql, new { UserId = userId });
        }

        public async Task<int> IncreaseBalanceAsync(int userId, int amount)
        {
            var sql = "UPDATE wallets SET balance = balance + @Amount WHERE user_id = @UserId";
            return await _databaseContext.ExecuteAsync(sql, new { UserId = userId, Amount = amount });
        }

        public async Task<int> DecreaseBalanceAsync(int userId, int amount)
        {
            var sql = "UPDATE wallets SET balance = balance - @Amount WHERE user_id = @UserId AND balance >= @Amount";
            return await _databaseContext.ExecuteAsync(sql, new { UserId = userId, Amount = amount });
        }
    }
}
