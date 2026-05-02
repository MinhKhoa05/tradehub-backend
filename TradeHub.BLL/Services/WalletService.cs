using TradeHub.BLL.Common;
using TradeHub.BLL.Exceptions;
using TradeHub.DAL;
using TradeHub.DAL.Entities;
using TradeHub.DAL.Repositories;
using TradeHub.DAL.Repositories.Interfaces;
using TradeHub.BLL.DTOs.Wallets;

namespace TradeHub.BLL.Services
{
    public class WalletService
    {
        private readonly IUserRepository _userRepo;
        private readonly IWalletTransactionRepository _walletTxRepo;
        private readonly DatabaseContext _database;

        public WalletService(IUserRepository userRepo, IWalletTransactionRepository walletTxRepo, DatabaseContext database)
        {
            _userRepo = userRepo;
            _walletTxRepo = walletTxRepo;
            _database = database;
        }

        /// <summary>
        /// Thực hiện trừ tiền ví của người dùng.
        /// Cần thực hiện trừ tiền trước khi tạo đơn hàng để đảm bảo khách hàng đủ khả năng thanh toán, 
        /// tránh tình trạng giữ hàng ảo hoặc tạo đơn hàng lỗi do thiếu vốn.
        /// </summary>
        public async Task<TransactionResponseDTO> DeductMoneyAsync(UserContext context, decimal amount, string description)
        {
            if (amount <= 0)
            {
                throw new BusinessException("Số tiền trừ phải lớn hơn 0.");
            }

            var user = await _userRepo.GetByIdAsync(context.UserId);
            if (user == null)
            {
                throw new BusinessException("Người dùng không tồn tại.");
            }

            if (user.Balance < amount)
            {
                throw new BusinessException("Số dư ví không đủ để thực hiện giao dịch này.");
            }

            return await _database.ExecuteInTransactionAsync(async () =>
            {
                var affected = await _userRepo.DecreaseBalanceAsync(context.UserId, amount);
                if (affected == 0)
                {
                    throw new BusinessException("Không thể cập nhật số dư ví.");
                }

                var txId = await _walletTxRepo.CreateAsync(new WalletTransaction
                {
                    UserId = context.UserId,
                    Amount = -amount,
                    BalanceAfter = user.Balance - amount,
                    Type = WalletTransactionType.PaidOrder,
                    Description = description,
                    CreatedAt = DateTime.UtcNow
                });

                return new TransactionResponseDTO { TransactionId = txId };
            });
        }

        public async Task<TransactionResponseDTO> RefundMoneyAsync(UserContext context, decimal amount, string description)
        {
            var user = await _userRepo.GetByIdAsync(context.UserId);
            if (user == null)
            {
                throw new BusinessException("Người dùng không tồn tại.");
            }

            return await _database.ExecuteInTransactionAsync(async () =>
            {
                await _userRepo.IncreaseBalanceAsync(context.UserId, amount);
                
                var txId = await _walletTxRepo.CreateAsync(new WalletTransaction
                {
                    UserId = context.UserId,
                    Amount = amount,
                    BalanceAfter = user.Balance + amount,
                    Type = WalletTransactionType.Refund,
                    Description = description,
                    CreatedAt = DateTime.UtcNow
                });
                return new TransactionResponseDTO { TransactionId = txId };
            });
        }

        public async Task<decimal> GetBalanceAsync(UserContext context)
        {
            var user = await _userRepo.GetByIdAsync(context.UserId);
            return user?.Balance ?? 0;
        }

        public async Task<List<WalletTransaction>> GetTransactionsAsync(UserContext context)
        {
            return await _walletTxRepo.GetByUserIdAsync(context.UserId);
        }

        public async Task<TransactionResponseDTO> DepositAsync(UserContext context, int amount)
        {
            var user = await _userRepo.GetByIdAsync(context.UserId);
            if (user == null)
            {
                throw new BusinessException("Người dùng không tồn tại.");
            }

            return await _database.ExecuteInTransactionAsync(async () =>
            {
                await _userRepo.IncreaseBalanceAsync(context.UserId, (decimal)amount);
                
                var txId = await _walletTxRepo.CreateAsync(new WalletTransaction
                {
                    UserId = context.UserId,
                    Amount = amount,
                    BalanceAfter = user.Balance + amount,
                    Type = WalletTransactionType.Deposit,
                    Description = $"Nạp tiền vào ví: {amount:N0} VNĐ",
                    CreatedAt = DateTime.UtcNow
                });
                return new TransactionResponseDTO { TransactionId = txId };
            });
        }

        public async Task<TransactionResponseDTO> WithdrawAsync(UserContext context, int amount)
        {
            var user = await _userRepo.GetByIdAsync(context.UserId);
            if (user == null)
            {
                throw new BusinessException("Người dùng không tồn tại.");
            }

            if (user.Balance < amount)
            {
                throw new BusinessException("Số dư không đủ để thực hiện rút tiền.");
            }

            return await _database.ExecuteInTransactionAsync(async () =>
            {
                await _userRepo.DecreaseBalanceAsync(context.UserId, (decimal)amount);
                
                var txId = await _walletTxRepo.CreateAsync(new WalletTransaction
                {
                    UserId = context.UserId,
                    Amount = -amount,
                    BalanceAfter = user.Balance - amount,
                    Type = WalletTransactionType.Withdraw,
                    Description = $"Rút tiền từ ví: {amount:N0} VNĐ",
                    CreatedAt = DateTime.UtcNow
                });
                return new TransactionResponseDTO { TransactionId = txId };
            });
        }

        public async Task<TransactionResponseDTO> PayForOrdersAsync(UserContext context, List<long> orderIds, int totalAmount)
        {
            var description = $"Thanh toán đơn hàng: {string.Join(", ", orderIds)}";
            return await DeductMoneyAsync(context, (decimal)totalAmount, description);
        }
    }
}