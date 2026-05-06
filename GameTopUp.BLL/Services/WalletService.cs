using GameTopUp.BLL.Common;
using GameTopUp.BLL.Exceptions;
using GameTopUp.DAL;
using GameTopUp.DAL.Entities;
using GameTopUp.DAL.Repositories;
using GameTopUp.DAL.Interfaces;
using GameTopUp.BLL.DTOs.Wallets;

namespace GameTopUp.BLL.Services
{
    public class WalletService
    {
        private readonly IWalletRepository _walletRepo;
        private readonly IWalletTransactionRepository _walletTxRepo;
        private readonly DatabaseContext _database;

        public WalletService(IWalletRepository walletRepo, IWalletTransactionRepository walletTxRepo, DatabaseContext database)
        {
            _walletRepo = walletRepo;
            _walletTxRepo = walletTxRepo;
            _database = database;
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
                // Rationale: Race condition safety. Even if the check above passes, 
                // the database constraint will prevent duplicate creation.
                throw new BusinessException("Ví của bạn đã được kích hoạt hoặc đang được xử lý.");
            }
        }

        /// <summary>
        /// Thực hiện trừ tiền ví của người dùng.
        /// Cần thực hiện trừ tiền trước khi tạo đơn hàng để đảm bảo khách hàng đủ khả năng thanh toán, 
        /// tránh tình trạng giữ hàng ảo hoặc tạo đơn hàng lỗi do thiếu vốn.
        /// </summary>
        public async Task<TransactionResponseDTO> DeductMoneyAsync(UserContext context, decimal amount, string description, long? orderId = null)
        {
            if (amount <= 0)
            {
                throw new BusinessException("Số tiền trừ phải lớn hơn 0.");
            }

            var wallet = await GetOrThrowByUserIdAsync(context.UserId);

            if (wallet.Balance < amount)
            {
                throw new BusinessException("Số dư ví không đủ để thực hiện giao dịch này.");
            }

            return await _database.ExecuteInTransactionAsync(async () =>
            {
                var affected = await _walletRepo.DecreaseBalanceAsync(context.UserId, amount);
                if (affected == 0)
                {
                    throw new BusinessException("Không thể cập nhật số dư ví hoặc số dư không đủ.");
                }

                var txId = await _walletTxRepo.CreateAsync(new WalletTransaction
                {
                    UserId = context.UserId,
                    Amount = -amount,
                    BalanceAfter = wallet.Balance - amount,
                    Type = WalletTransactionType.PaidOrder,
                    Description = description,
                    OrderId = orderId,
                    CreatedAt = DateTime.UtcNow
                });

                return new TransactionResponseDTO { TransactionId = txId };
            });
        }

        public async Task<TransactionResponseDTO> RefundMoneyAsync(UserContext context, decimal amount, string description, long? orderId = null)
        {
            var wallet = await GetOrThrowByUserIdAsync(context.UserId);

            return await _database.ExecuteInTransactionAsync(async () =>
            {
                await _walletRepo.IncreaseBalanceAsync(context.UserId, amount);
                
                var txId = await _walletTxRepo.CreateAsync(new WalletTransaction
                {
                    UserId = context.UserId,
                    Amount = amount,
                    BalanceAfter = wallet.Balance + amount,
                    Type = WalletTransactionType.Refund,
                    Description = description,
                    OrderId = orderId,
                    CreatedAt = DateTime.UtcNow
                });
                return new TransactionResponseDTO { TransactionId = txId };
            });
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

        public async Task<TransactionResponseDTO> DepositAsync(UserContext context, decimal amount)
        {
            var wallet = await GetOrThrowByUserIdAsync(context.UserId);

            return await _database.ExecuteInTransactionAsync(async () =>
            {
                await _walletRepo.IncreaseBalanceAsync(context.UserId, amount);
                
                var txId = await _walletTxRepo.CreateAsync(new WalletTransaction
                {
                    UserId = context.UserId,
                    Amount = amount,
                    BalanceAfter = wallet.Balance + amount,
                    Type = WalletTransactionType.Deposit,
                    Description = $"Nạp tiền vào ví: {amount:N0} VNĐ",
                    CreatedAt = DateTime.UtcNow
                });
                return new TransactionResponseDTO { TransactionId = txId };
            });
        }

        public async Task<TransactionResponseDTO> WithdrawAsync(UserContext context, decimal amount)
        {
            var wallet = await GetOrThrowByUserIdAsync(context.UserId);

            if (wallet.Balance < amount)
            {
                throw new BusinessException("Số dư không đủ để thực hiện rút tiền.");
            }

            return await _database.ExecuteInTransactionAsync(async () =>
            {
                var affected = await _walletRepo.DecreaseBalanceAsync(context.UserId, amount);
                if (affected == 0)
                {
                    throw new BusinessException("Không thể cập nhật số dư ví hoặc số dư không đủ.");
                }
                
                var txId = await _walletTxRepo.CreateAsync(new WalletTransaction
                {
                    UserId = context.UserId,
                    Amount = -amount,
                    BalanceAfter = wallet.Balance - amount,
                    Type = WalletTransactionType.Withdraw,
                    Description = $"Rút tiền từ ví: {amount:N0} VNĐ",
                    CreatedAt = DateTime.UtcNow
                });
                return new TransactionResponseDTO { TransactionId = txId };
            });
        }

        // --------------- Private Helpers ---------------

        /// <summary>
        /// Lấy ví của người dùng theo UserId. Ném <see cref="NotFoundException"/> nếu ví chưa được kích hoạt.
        /// </summary>
        private async Task<Wallet> GetOrThrowByUserIdAsync(long userId)
        {
            return await _walletRepo.GetByUserIdAsync(userId)
                ?? throw new NotFoundException("Ví của bạn chưa được kích hoạt. Vui lòng kích hoạt ví để sử dụng.");
        }
    }
}
