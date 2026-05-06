using GameTopUp.BLL.Common;
using GameTopUp.BLL.DTOs.Wallets;
using GameTopUp.BLL.Services;
using GameTopUp.DAL;
using GameTopUp.DAL.Entities;

namespace GameTopUp.BLL.ApplicationServices
{
    public class WalletUseCase
    {
        private readonly WalletService _walletService;
        private readonly DatabaseContext _database;

        public WalletUseCase(WalletService walletService, DatabaseContext database)
        {
            _walletService = walletService;
            _database = database;
        }

        public async Task<long> ActiveWalletAsync(UserContext context)
        {
            return await _database.ExecuteInTransactionAsync(async () =>
            {
                return await _walletService.CreateWalletAsync(context);
            });
        }

        public async Task<TransactionResponseDTO> DepositAsync(UserContext context, decimal amount)
        {
            return await _database.ExecuteInTransactionAsync(async () =>
            {
                var wallet = await _walletService.LockAndGetByUserIdAsync(context.UserId);

                return await _walletService.CreditAsync(
                    wallet, 
                    amount, 
                    WalletTransactionType.Deposit, 
                    $"Nạp tiền vào ví: {amount:N0} VNĐ");
            });
        }

        public async Task<TransactionResponseDTO> WithdrawAsync(UserContext context, decimal amount)
        {
            return await _database.ExecuteInTransactionAsync(async () =>
            {
                var wallet = await _walletService.LockAndGetByUserIdAsync(context.UserId);

                return await _walletService.DebitAsync(
                    wallet, 
                    amount, 
                    WalletTransactionType.Withdraw, 
                    $"Rút tiền từ ví: {amount:N0} VNĐ");
            });
        }
    }
}
