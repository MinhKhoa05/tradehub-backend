using TradeHub.DAL.Entities;

namespace TradeHub.DAL.Repositories
{
    public class WalletTransactionRepository
    {
        private readonly DatabaseContext _databaseContext;

        public WalletTransactionRepository(DatabaseContext databaseContext)
        {
            _databaseContext = databaseContext;
        }

        public async Task<List<WalletTransaction>> GetByWalletIdAsync(int walletId)
        {
            var sql = "SELECT * FROM wallet_transactions WHERE wallet_id = @WalletId ORDER BY created_at DESC";
            return await _databaseContext.QueryListAsync<WalletTransaction>(sql, new { WalletId = walletId });
        }
        
        public async Task<int> CreateAsync(WalletTransaction walletTransaction)
        {
            var sql = @"INSERT INTO wallet_transactions (wallet_id, amount, type, reference_id, description)
                        VALUES (@WalletId, @Amount, @Type, @ReferenceId, @Description)";
            return await _databaseContext.ExecuteInsertAsync(sql, walletTransaction);
        }
    }
}
