using TradeHub.DAL.Repositories;
using TradeHub.DAL.Entities;
using TradeHub.BLL.Exceptions;
using TradeHub.DAL;
using TradeHub.BLL.DTOs.Wallets;

namespace TradeHub.BLL.Services
{
    public class WalletService
    {
        private readonly WalletRepository _walletRepository;
        private readonly WalletTransactionRepository _walletTransactionRepository;
        private readonly DatabaseContext _databaseContext;

        public WalletService(WalletRepository walletRepository, WalletTransactionRepository walletTransactionRepository, DatabaseContext databaseContext)
        {
            _walletRepository = walletRepository;
            _walletTransactionRepository = walletTransactionRepository;
            _databaseContext = databaseContext;
        }

        public async Task<List<WalletTransaction>> GetWalletTransactionsAsync(int userId)
        {
            var wallet = await GetWalletByUserIdOrThrowAsync(userId);

            return await _walletTransactionRepository.GetByWalletIdAsync(wallet.Id);
        }

        public async Task<int> GetWalletBalanceAsync(int userId)
        {
            var wallet = await GetWalletByUserIdOrThrowAsync(userId);
            return wallet.Balance;
        }

        public async Task<Wallet> CreateWalletAsync(int userId)
        {
            var wallet = new Wallet
            {
                UserId = userId,
                Balance = 0,
            };

            wallet.Id = await _walletRepository.CreateAsync(wallet);
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

        public async Task<WalletTransaction> PayOrderAsync(int userId, int orderId, int totalAmount)
        {
            var transactionInfo = new WalletTransactionInfo
            {
                Type = WalletTransactionType.PaidOrder,
                Description = $"Thanh toán đơn hàng #{orderId}, Số tiền {totalAmount}",
                ReferenceId = orderId,
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

            await _databaseContext.ExecuteInTransactionAsync(async () =>
            {
                if (info.IsDecreaseBalance)
                {
                    await DecreaseBalanceOrThrowAsync(userId, amount);
                } else
                {
                    await _walletRepository.IncreaseBalanceAsync(userId, amount);
                }

                walletTransaction.Id = await _walletTransactionRepository.CreateAsync(walletTransaction);
            });

            return walletTransaction;
        }

        public async Task<Wallet> GetWalletByUserIdOrThrowAsync(int userId)
        {
            var wallet = await _walletRepository.GetByUserIdAsync(userId);
            if (wallet == null)
            {
                throw new BusinessException($"Người dùng không có ví");
            }
            return wallet;
        }

        private async Task DecreaseBalanceOrThrowAsync(int userId, int amount)
        {
            var affected = await _walletRepository.DecreaseBalanceAsync(userId, amount);

            if (affected == 0)
                throw new BusinessException("Số dư không đủ để thực hiện");
        }
    }
}
