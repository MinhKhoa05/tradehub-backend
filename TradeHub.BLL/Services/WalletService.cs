using TradeHub.BLL.Common;
using TradeHub.BLL.DTOs.Wallets;
using TradeHub.BLL.Exceptions;
using TradeHub.DAL;
using TradeHub.DAL.Entities;
using TradeHub.DAL.Repositories;

namespace TradeHub.BLL.Services
{
    public class WalletService : BaseService
    {
        private readonly WalletRepository _walletRepo;
        private readonly WalletTransactionRepository _walletTxRepo;
        private readonly DatabaseContext _database;

        public WalletService(
            WalletRepository walletRepo,
            WalletTransactionRepository walletTxRepo,
            DatabaseContext database,
            IIdentityService identityService)
            : base(identityService)
        {
            _walletRepo = walletRepo;
            _walletTxRepo = walletTxRepo;
            _database = database;
        }

        // ===== READ (Lấy dữ liệu của Tôi) =====

        public async Task<List<WalletTransaction>> GetMyTransactionsAsync()
        {
            var wallet = await GetOrCreateWalletInternalAsync();
            return await _walletTxRepo.GetByWalletIdAsync(wallet.Id);
        }

        public async Task<int> GetMyBalanceAsync()
        {
            var wallet = await GetOrCreateWalletInternalAsync();
            return wallet.Balance;
        }

        // ===== ACTIONS (Thao tác nghiệp vụ thuần túy) =====

        public async Task EnsureBalanceIsEnoughAsync(int requiredAmount)
        {
            var balance = await GetMyBalanceAsync();

            if (balance < requiredAmount)
                throw new BusinessException("Số dư ví không đủ để thực hiện giao dịch này.");
        }

        public async Task<WalletTransaction> DepositAsync(int amount)
        {
            var info = new WalletTransactionInfo
            {
                Type = WalletTransactionType.Deposit,
                Description = $"Nạp tiền vào ví: {amount:N0} VNĐ",
            };

            return await ProcessTransactionInternalAsync(amount, info);
        }

        public async Task<WalletTransaction> WithdrawAsync(int amount)
        {
            var info = new WalletTransactionInfo
            {
                Type = WalletTransactionType.Withdraw,
                Description = $"Rút tiền từ ví: {amount:N0} VNĐ",
                IsDecreaseBalance = true
            };

            return await ProcessTransactionInternalAsync(amount, info);
        }

        public async Task<WalletTransaction> PayForOrdersAsync(List<int> orderIds, int totalAmount)
        {
            if (orderIds == null || !orderIds.Any())
                throw new BusinessException("Không có đơn hàng nào để thanh toán.");

            var info = new WalletTransactionInfo
            {
                Type = WalletTransactionType.PaidOrder,
                Description = $"Thanh toán {orderIds.Count} đơn hàng (Mã: {string.Join(", ", orderIds)}). " +
                              $"Tổng tiền: {totalAmount:N0} VNĐ",
                IsDecreaseBalance = true
            };

            return await ProcessTransactionInternalAsync(totalAmount, info);
        }

        // ===== INTERNAL / PRIVATE (Xử lý ngầm & Bảo mật) =====

        private async Task<WalletTransaction> ProcessTransactionInternalAsync(int amount, WalletTransactionInfo info)
        {
            if (amount <= 0)
                throw new BusinessException("Số tiền giao dịch phải lớn hơn 0.");

            var wallet = await GetOrCreateWalletInternalAsync();

            var tx = new WalletTransaction
            {
                WalletId = wallet.Id,
                Amount = info.IsDecreaseBalance ? -amount : amount,
                Type = info.Type,
                Description = info.Description,
                ReferenceId = info.ReferenceId,
            };

            // Dùng Transaction để đảm bảo tính toàn vẹn dữ liệu
            await _database.ExecuteInTransactionAsync(async () =>
            {
                if (info.IsDecreaseBalance)
                {
                    // Chặn Race Condition bằng affected rows (Fail-fast xịn)
                    var affected = await _walletRepo.DecreaseBalanceAsync(CurrentUserId, amount);

                    if (affected == 0)
                        throw new BusinessException("Giao dịch thất bại: Số dư không đủ.");
                }
                else
                {
                    await _walletRepo.IncreaseBalanceAsync(CurrentUserId, amount);
                }

                // Lưu nhật ký giao dịch
                tx.Id = await _walletTxRepo.CreateAsync(tx);
            });

            return tx;
        }

        private async Task<Wallet> GetOrCreateWalletInternalAsync()
        {
            var wallet = await _walletRepo.GetByUserIdAsync(CurrentUserId);

            if (wallet == null)
            {
                // Tự động khởi tạo ví nếu User chưa có
                wallet = new Wallet
                {
                    UserId = CurrentUserId,
                    Balance = 0
                };
                wallet.Id = await _walletRepo.CreateAsync(wallet);
            }

            return wallet;
        }
    }
}