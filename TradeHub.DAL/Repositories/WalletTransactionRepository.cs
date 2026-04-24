using TradeHub.DAL.Entities;
using TradeHub.DAL.Repositories.Interfaces;

namespace TradeHub.DAL.Repositories
{
    public class WalletTransactionRepository : IWalletTransactionRepository
    {
        private readonly DatabaseContext _database;

        public WalletTransactionRepository(DatabaseContext database)
        {
            _database = database;
        }

        public async Task<List<WalletTransaction>> GetByUserIdAsync(long userId)
        {
            var sql = "SELECT * FROM wallet_transactions WHERE user_id = @UserId ORDER BY created_at DESC";
            return await _database.QueryAsync<WalletTransaction>(sql, new { UserId = userId });
        }
        
        public async Task<long> CreateAsync(WalletTransaction walletTransaction)
        {
            return await _database.InsertAsync<WalletTransaction, long>(walletTransaction);
        }
    }
}
