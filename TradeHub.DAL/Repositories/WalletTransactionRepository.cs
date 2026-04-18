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

        public async Task<List<WalletTransaction>> GetByWalletIdAsync(long walletId)
        {
            var sql = "SELECT * FROM wallet_transactions WHERE wallet_id = @WalletId ORDER BY created_at DESC";
            return await _database.SqlQueryAsync<WalletTransaction>(sql, new { WalletId = walletId });
        }
        
        public async Task<long> CreateAsync(WalletTransaction walletTransaction)
        {
            return await _database.InsertAsync(walletTransaction);
        }
    }
}
