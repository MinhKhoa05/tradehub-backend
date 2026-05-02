using GameTopUp.DAL.Entities;
using GameTopUp.DAL.Interfaces;

namespace GameTopUp.DAL.Repositories
{
    public class WalletRepository : IWalletRepository
    {
        private readonly DatabaseContext _database;

        public WalletRepository(DatabaseContext database)
        {
            _database = database;
        }

        public async Task<Wallet?> GetByUserIdAsync(long userId)
        {
            string sql = "SELECT * FROM wallets WHERE user_id = @UserId";
            return await _database.QueryFirstAsync<Wallet>(sql, new { UserId = userId });
        }

        public async Task<long> CreateAsync(Wallet wallet)
        {
            return await _database.InsertAsync<Wallet, long>(wallet);
        }

        public async Task<int> IncreaseBalanceAsync(long userId, decimal amount)
        {
            string sql = "UPDATE wallets SET balance = balance + @Amount, updated_at = @UpdatedAt WHERE user_id = @UserId";
            return await _database.ExecuteAsync(sql, new 
            { 
                UserId = userId, 
                Amount = amount,
                UpdatedAt = DateTime.UtcNow
            });
        }

        public async Task<int> DecreaseBalanceAsync(long userId, decimal amount)
        {
            string sql = "UPDATE wallets SET balance = balance - @Amount, updated_at = @UpdatedAt WHERE user_id = @UserId AND balance >= @Amount";
            return await _database.ExecuteAsync(sql, new 
            { 
                UserId = userId, 
                Amount = amount,
                UpdatedAt = DateTime.UtcNow
            });
        }
    }
}
