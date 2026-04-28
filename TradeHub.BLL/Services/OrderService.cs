using TradeHub.DAL;
using TradeHub.DAL.Entities;
using TradeHub.DAL.Repositories.Interfaces;
using TradeHub.BLL.Common;

namespace TradeHub.BLL.Services
{
    public class OrderService : BaseService
    {
        private readonly IOrderRepository _orderRepo;
        private readonly IOrderHistoryRepository _orderHistoryRepo;
        private readonly DatabaseContext _database;

        public OrderService(
            IOrderRepository orderRepo,
            IOrderHistoryRepository orderHistoryRepo,
            DatabaseContext database,
            IIdentityService identityService)
            : base(identityService)
        {
            _orderRepo = orderRepo;
            _orderHistoryRepo = orderHistoryRepo;
            _database = database;
        }

        /// <summary>
        /// Tạo đơn hàng mới và lưu lịch sử
        /// </summary>
        /// <param name="order">Dữ liệu đơn hàng</param>
        /// <param name="walletTransactionId">ID giao dịch ví liên quan</param>
        /// <returns>ID đơn hàng vừa tạo</returns>
        public async Task<long> CreateOrderAsync(Order order, long walletTransactionId)
        {
            order.WalletTransactionId = walletTransactionId;
            order.Status = OrderStatus.Pending;
            order.CreatedAt = DateTime.UtcNow;
            order.UpdatedAt = DateTime.UtcNow;

            return await _database.ExecuteInTransactionAsync(async () =>
            {
                // 1. Lưu đơn hàng
                var orderId = await _orderRepo.CreateAsync(order);
                order.Id = orderId;

                // 2. Lưu lịch sử đơn hàng
                var history = new OrderHistory
                {
                    OrderId = orderId,
                    FromStatus = OrderStatus.Pending,
                    ToStatus = OrderStatus.Pending,
                    Note = "Đơn hàng được tạo thành công.",
                    ActionBy = order.UserId,
                    IsAdmin = false,
                    CreatedAt = DateTime.UtcNow
                };

                await _orderHistoryRepo.CreateAsync(history);

                return orderId;
            });
        }

        /// <summary>
        /// Lấy danh sách đơn hàng của người dùng hiện tại
        /// </summary>
        public async Task<List<Order>> GetMyOrdersAsync(OrderStatus? status = null)
        {
            return await _orderRepo.GetByUserIdAsync(CurrentUserId, status);
        }

        /// <summary>
        /// Lấy lịch sử thay đổi trạng thái của một đơn hàng
        /// </summary>
        public async Task<List<OrderHistory>> GetHistoriesAsync(long orderId)
        {
            return await _orderHistoryRepo.GetByOrderIdAsync(orderId);
        }

        /// <summary>
        /// Lấy thông tin chi tiết đơn hàng (Dùng chung bảng Order vì hiện tại 1 Order = 1 Item)
        /// </summary>
        public async Task<Order?> GetItemsAsync(long orderId)
        {
            return await _orderRepo.GetByIdAsync(orderId);
        }
    }
}
