using GameTopUp.DAL.Entities;
using GameTopUp.DAL.Interfaces;

namespace GameTopUp.DAL.Repositories
{
    /// <summary>
    /// Repository quản lý biến động số dư ví (Nạp, Rút, Thanh toán).
    /// </summary>
    public class WalletTransactionRepository : IWalletTransactionRepository
    {
        private readonly DatabaseContext _database;

        public WalletTransactionRepository(DatabaseContext database)
        {
            _database = database;
        }

        public async Task<List<WalletTransaction>> GetByUserIdAsync(long userId)
        {
            // Hiển thị các giao dịch mới nhất lên trên cùng để người dùng dễ kiểm soát chi tiêu.
            var sql = "SELECT * FROM wallet_transactions WHERE user_id = @UserId ORDER BY created_at DESC";
            
            return await _database.QueryAsync<WalletTransaction>(sql, new 
            { 
                UserId = userId 
            });
        }
        
        public async Task<long> CreateAsync(WalletTransaction walletTransaction)
        {
            return await _database.InsertAsync<WalletTransaction, long>(walletTransaction);
        }
    }
}
