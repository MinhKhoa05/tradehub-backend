using GameTopUp.BLL.Common;
using GameTopUp.BLL.Exceptions;
using GameTopUp.DAL;
using GameTopUp.DAL.Entities;
using GameTopUp.DAL.Interfaces;
using GameTopUp.BLL.DTOs.Wallets;

namespace GameTopUp.BLL.Services
{
    public class WalletService
    {
        private readonly IWalletRepository _walletRepo;
        private readonly IWalletTransactionRepository _walletTxRepo;

        public WalletService(IWalletRepository walletRepo, IWalletTransactionRepository walletTxRepo)
        {
            _walletRepo = walletRepo;
            _walletTxRepo = walletTxRepo;
        }

        public async Task<long> CreateWalletAsync(UserContext context)
        {
            var existingWallet = await _walletRepo.GetByUserIdAsync(context.UserId);
            if (existingWallet != null)
            {
                throw new BusinessException("Ví của bạn đã được kích hoạt.");
            }

            try
            {
                return await _walletRepo.CreateAsync(new Wallet
                {
                    UserId = context.UserId,
                    Balance = 0,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex) when (ex.Message.Contains("Duplicate", StringComparison.OrdinalIgnoreCase) || 
                                       ex.Message.Contains("UNIQUE", StringComparison.OrdinalIgnoreCase))
            {
                throw new BusinessException("Ví của bạn đã được kích hoạt hoặc đang được xử lý.");
            }
        }

        public async Task<Wallet> LockAndGetByUserIdAsync(long userId)
        {
            return await _walletRepo.GetByUserIdForUpdateAsync(userId)
                ?? throw new NotFoundException("Ví của bạn chưa được kích hoạt. Vui lòng kích hoạt ví để sử dụng.");
        }

        /// <summary>
        /// Nạp tiền vào ví (Cộng số dư).
        /// </summary>
        public async Task<TransactionResponseDTO> CreditAsync(
            Wallet wallet, 
            decimal amount, 
            WalletTransactionType type, 
            string description, 
            long? orderId = null)
        {
            if (amount <= 0) throw new BusinessException("Số tiền nạp phải lớn hơn 0.");

            decimal balanceBefore = wallet.Balance;
            decimal balanceAfter = balanceBefore + amount;

            // 1. Cập nhật số dư (Atomic Update)
            var affected = await _walletRepo.IncreaseBalanceAsync(wallet.UserId, amount);
            if (affected == 0)
            {
                throw new BusinessException("Không thể cập nhật số dư ví. Vui lòng thử lại sau.");
            }

            // Đồng bộ lại object trong memory
            wallet.Balance = balanceAfter;
            wallet.UpdatedAt = DateTime.UtcNow;

            // 2. Ghi log giao dịch
            var txId = await _walletTxRepo.CreateAsync(new WalletTransaction
            {
                UserId = wallet.UserId,
                Amount = amount,
                BalanceBefore = balanceBefore,
                BalanceAfter = balanceAfter,
                Type = type,
                Description = description,
                OrderId = orderId,
                CreatedAt = DateTime.UtcNow
            });

            return new TransactionResponseDTO { TransactionId = txId };
        }

        /// <summary>
        /// Trừ tiền từ ví (Trừ số dư).
        /// </summary>
        public async Task<TransactionResponseDTO> DebitAsync(
            Wallet wallet, 
            decimal amount, 
            WalletTransactionType type, 
            string description, 
            long? orderId = null)
        {
            if (amount <= 0) throw new BusinessException("Số tiền trừ phải lớn hơn 0.");

            // Kiểm tra số dư trước khi trừ
            if (wallet.Balance < amount)
            {
                throw new BusinessException("Số dư ví không đủ để thực hiện giao dịch này.");
            }

            decimal balanceBefore = wallet.Balance;
            decimal balanceAfter = balanceBefore - amount;

            // 1. Cập nhật số dư (Atomic Update với kiểm tra số dư tại tầng DB)
            var affected = await _walletRepo.DecreaseBalanceAsync(wallet.UserId, amount);
            if (affected == 0)
            {
                throw new BusinessException("Không thể cập nhật số dư ví hoặc số dư không đủ.");
            }

            // Đồng bộ lại object trong memory
            wallet.Balance = balanceAfter;
            wallet.UpdatedAt = DateTime.UtcNow;

            // 2. Ghi log giao dịch (Số tiền âm để thể hiện việc trừ tiền)
            var txId = await _walletTxRepo.CreateAsync(new WalletTransaction
            {
                UserId = wallet.UserId,
                Amount = -amount,
                BalanceBefore = balanceBefore,
                BalanceAfter = balanceAfter,
                Type = type,
                Description = description,
                OrderId = orderId,
                CreatedAt = DateTime.UtcNow
            });

            return new TransactionResponseDTO { TransactionId = txId };
        }

        public async Task<decimal> GetBalanceAsync(UserContext context)
        {
            var wallet = await GetOrThrowByUserIdAsync(context.UserId);
            return wallet.Balance;
        }

        public async Task<List<WalletTransaction>> GetTransactionsAsync(UserContext context)
        {
            return await _walletTxRepo.GetByUserIdAsync(context.UserId);
        }

        public async Task<Wallet> GetOrThrowByUserIdAsync(long userId)
        {
            return await _walletRepo.GetByUserIdAsync(userId)
                ?? throw new NotFoundException("Ví của bạn chưa được kích hoạt. Vui lòng kích hoạt ví để sử dụng.");
        }
    }
}
