using TradeHub.DAL.Repositories;
using TradeHub.DAL.Entities;
using TradeHub.BLL.Exceptions;
using TradeHub.DAL;
using TradeHub.BLL.DTOs.Wallets;

namespace TradeHub.BLL.Services
{
    public class WalletService
    {
        private readonly WalletRepository _walletRepo;
        private readonly WalletTransactionRepository _walletTxRepo;
        private readonly DatabaseContext _database;
        
        public WalletService(WalletRepository walletRepo, WalletTransactionRepository walletTxRepo, DatabaseContext database)
        {
            _walletRepo = walletRepo;
            _walletTxRepo = walletTxRepo;
            _database = database;
        }
        
        public async Task<List<WalletTransaction>> GetWalletTransactionsAsync(int userId)
        {
            var wallet = await GetWalletByUserIdOrThrowAsync(userId);

            return await _walletTxRepo.GetByWalletIdAsync(wallet.Id);
        }

        public async Task<int> GetWalletBalanceAsync(int userId)
        {
            var wallet = await GetWalletByUserIdOrThrowAsync(userId);
            return wallet.Balance;
        }

        public async Task<Wallet> CreateWalletAsync(int userId)
        {
            var existing = await _walletRepo.GetByUserIdAsync(userId);
            if (existing != null)
            {
                throw new BusinessException("Người dùng đã có ví");
            }

            var wallet = new Wallet
            {
                UserId = userId,
                Balance = 0,
            };

            wallet.Id = await _walletRepo.CreateAsync(wallet);
            return wallet;
        }

        public async Task<WalletTransaction> DepositAsync(int userId, int amount)
        {
            var transactionInfo = new WalletTransactionInfo
            {
                Type = WalletTransactionType.Deposit,
                Description = $"Nạp tiền, số tiền {amount}",
            };

            return await ProcessTransactionAsync(userId, amount, transactionInfo);
        }

        public async Task<WalletTransaction> WithdrawAsync(int userId, int amount)
        {
            var transactionInfo = new WalletTransactionInfo
            {
                Type = WalletTransactionType.Withdraw,
                Description = $"Rút tiền, số tiền {amount}",
                IsDecreaseBalance = true
            };

            return await ProcessTransactionAsync(userId, amount, transactionInfo);
        }

        public async Task<WalletTransaction> PayForOrdersAsync(int userId, List<int> orderIds, int totalAmount)
        {
            if (orderIds == null || !orderIds.Any())
                throw new BusinessException("Không có đơn hàng để thanh toán");

            var transactionInfo = new WalletTransactionInfo
            {
                Type = WalletTransactionType.PaidOrder,
                // Ghi summary: số lượng đơn, list mã đơn, tổng tiền, ngày
                Description = $"Thanh toán {orderIds.Count} đơn hàng (Mã đơn: {string.Join(",", orderIds)}), " +
                              $"Tổng tiền {totalAmount}, Ngày {DateTime.Now:dd/MM/yyyy}",
                IsDecreaseBalance = true
            };

            return await ProcessTransactionAsync(userId, totalAmount, transactionInfo);
        }

        private async Task<WalletTransaction> ProcessTransactionAsync(int userId, int amount, WalletTransactionInfo info)
        {
            if (amount <= 0)
                throw new BusinessException("Số tiền phải lớn hơn 0");

            var wallet = await GetWalletByUserIdOrThrowAsync(userId);

            var walletTransaction = new WalletTransaction
            {
                WalletId = wallet.Id,
                Amount = info.IsDecreaseBalance ? -amount : amount,
                Type = info.Type,
                Description = info.Description,
                ReferenceId = info.ReferenceId,
            };

            await _database.ExecuteInTransactionAsync(async () =>
            {
                if (info.IsDecreaseBalance)
                {
                    await DecreaseBalanceOrThrowAsync(userId, amount);
                } else
                {
                    await _walletRepo.IncreaseBalanceAsync(userId, amount);
                }

                walletTransaction.Id = await _walletTxRepo.CreateAsync(walletTransaction);
            });

            return walletTransaction;
        }

        public async Task<Wallet> GetWalletByUserIdOrThrowAsync(int userId)
        {
            var wallet = await _walletRepo.GetByUserIdAsync(userId)
                            ?? throw new BusinessException("Người dùng chưa mở ví");

            return wallet;
        }

        private async Task DecreaseBalanceOrThrowAsync(int userId, int amount)
        {
            var affected = await _walletRepo.DecreaseBalanceAsync(userId, amount);

            if (affected == 0)
                throw new BusinessException("Số dư không đủ để thực hiện");
        }
    }
}
