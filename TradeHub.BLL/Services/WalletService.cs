using TradeHub.BLL.Common;
using TradeHub.BLL.Exceptions;
using TradeHub.DAL;
using TradeHub.DAL.Entities;
using TradeHub.DAL.Repositories;
using TradeHub.DAL.Repositories.Interfaces;


namespace TradeHub.BLL.Services
{
    public class WalletService : BaseService
    {
        private readonly UserRepository _userRepo;
        private readonly IWalletTransactionRepository _walletTxRepo;
        private readonly DatabaseContext _database;

        public WalletService(
            UserRepository userRepo,
            IWalletTransactionRepository walletTxRepo,
            DatabaseContext database,
            IIdentityService identityService)
            : base(identityService)
        {
            _userRepo = userRepo;
            _walletTxRepo = walletTxRepo;
            _database = database;
        }

        /// <summary>
        /// Thực hiện trừ tiền từ ví người dùng
        /// </summary>
        /// <param name="userId">ID người dùng</param>
        /// <param name="amount">Số tiền cần trừ</param>
        /// <param name="description">Mô tả giao dịch</param>
        /// <returns>ID của giao dịch ví</returns>
        public async Task<long> DeductMoneyAsync(long userId, decimal amount, string description)
        {
            if (amount <= 0)
                throw new BusinessException("Số tiền trừ phải lớn hơn 0.");

            var user = await _userRepo.GetByIdAsync(userId);
            if (user == null)
                throw new BusinessException("Người dùng không tồn tại.");

            if (user.Balance < amount)
                throw new BusinessException("Số dư ví không đủ để thực hiện giao dịch.");

            return await _database.ExecuteInTransactionAsync(async () =>
            {
                // Trừ tiền
                var affected = await _userRepo.DecreaseBalanceAsync(userId, amount);
                if (affected == 0)
                    throw new BusinessException("Trừ tiền thất bại. Vui lòng kiểm tra lại số dư.");

                // Lưu lịch sử giao dịch
                var tx = new WalletTransaction
                {
                    UserId = userId,
                    Amount = -amount,
                    BalanceAfter = user.Balance - amount,
                    Type = WalletTransactionType.PaidOrder,
                    Description = description,
                    CreatedAt = DateTime.UtcNow
                };

                return await _walletTxRepo.CreateAsync(tx);
            });
        }

        /// <summary>
        /// Hoàn tiền cho người dùng (Dùng trong trường hợp Rollback thủ công nếu cần)
        /// </summary>
        public async Task<long> RefundMoneyAsync(long userId, decimal amount, string description)
        {
            var user = await _userRepo.GetByIdAsync(userId);
            if (user == null) throw new BusinessException("Người dùng không tồn tại.");

            return await _database.ExecuteInTransactionAsync(async () =>
            {
                await _userRepo.IncreaseBalanceAsync(userId, amount);

                var tx = new WalletTransaction
                {
                    UserId = userId,
                    Amount = amount,
                    BalanceAfter = user.Balance + amount,
                    Type = WalletTransactionType.Refund,
                    Description = description,
                    CreatedAt = DateTime.UtcNow
                };

                return await _walletTxRepo.CreateAsync(tx);
            });
        }

        public async Task<decimal> GetBalanceAsync(long userId)
        {
            var user = await _userRepo.GetByIdAsync(userId);
            return user?.Balance ?? 0;
        }

        public async Task<decimal> GetMyBalanceAsync()
        {
            return await GetBalanceAsync(CurrentUserId);
        }

        public async Task<List<WalletTransaction>> GetMyTransactionsAsync()
        {
            return await _walletTxRepo.GetByUserIdAsync(CurrentUserId);
        }

        public async Task<long> DepositAsync(int amount)
        {
            return await _database.ExecuteInTransactionAsync(async () =>
            {
                var user = await _userRepo.GetByIdAsync(CurrentUserId);
                if (user == null) throw new BusinessException("Người dùng không tồn tại.");

                await _userRepo.IncreaseBalanceAsync(CurrentUserId, (decimal)amount);

                var tx = new WalletTransaction
                {
                    UserId = CurrentUserId,
                    Amount = amount,
                    BalanceAfter = user.Balance + amount,
                    Type = WalletTransactionType.Deposit,
                    Description = $"Nạp tiền vào ví: {amount:N0} VNĐ",
                    CreatedAt = DateTime.UtcNow
                };

                return await _walletTxRepo.CreateAsync(tx);
            });
        }

        public async Task<long> WithdrawAsync(int amount)
        {
            return await _database.ExecuteInTransactionAsync(async () =>
            {
                var user = await _userRepo.GetByIdAsync(CurrentUserId);
                if (user == null) throw new BusinessException("Người dùng không tồn tại.");
                if (user.Balance < amount) throw new BusinessException("Số dư không đủ.");

                await _userRepo.DecreaseBalanceAsync(CurrentUserId, (decimal)amount);

                var tx = new WalletTransaction
                {
                    UserId = CurrentUserId,
                    Amount = -amount,
                    BalanceAfter = user.Balance - amount,
                    Type = WalletTransactionType.Withdraw,
                    Description = $"Rút tiền từ ví: {amount:N0} VNĐ",
                    CreatedAt = DateTime.UtcNow
                };

                return await _walletTxRepo.CreateAsync(tx);
            });
        }

        public async Task<long> PayForOrdersAsync(List<long> orderIds, int totalAmount)
        {
            var description = $"Thanh toán đơn hàng: {string.Join(", ", orderIds)}";
            return await DeductMoneyAsync(CurrentUserId, (decimal)totalAmount, description);
        }
    }
}