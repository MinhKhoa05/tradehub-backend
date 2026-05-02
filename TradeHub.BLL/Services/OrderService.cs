using TradeHub.DAL;
using TradeHub.DAL.Entities;
using TradeHub.DAL.Interfaces;
using TradeHub.BLL.Common;

namespace TradeHub.BLL.Services
{
    public class OrderService
    {
        private readonly IOrderRepository _orderRepo;
        private readonly IOrderHistoryRepository _orderHistoryRepo;
        private readonly DatabaseContext _database;

        public OrderService(IOrderRepository orderRepo, IOrderHistoryRepository orderHistoryRepo, DatabaseContext database)
        {
            _orderRepo = orderRepo;
            _orderHistoryRepo = orderHistoryRepo;
            _database = database;
        }

        public async Task<long> CreateOrderAsync(Order order, long walletTransactionId)
        {
            order.WalletTransactionId = walletTransactionId;
            order.Status = OrderStatus.Pending;
            order.CreatedAt = order.UpdatedAt = DateTime.UtcNow;

            return await _database.ExecuteInTransactionAsync(async () =>
            {
                var orderId = await _orderRepo.CreateAsync(order);
                
                // Lưu vết lịch sử trạng thái ngay khi tạo đơn để dễ dàng truy vết (Audit Log)
                await _orderHistoryRepo.CreateAsync(new OrderHistory
                {
                    OrderId = orderId,
                    FromStatus = OrderStatus.Pending,
                    ToStatus = OrderStatus.Pending,
                    Note = "Đơn hàng được tạo thành công và đang chờ xử lý.",
                    ActionBy = order.UserId,
                    CreatedAt = DateTime.UtcNow
                });
                
                return orderId;
            });
        }

        public async Task<List<Order>> GetOrdersAsync(UserContext context, OrderStatus? status = null)
        {
            return await _orderRepo.GetByUserIdAsync(context.UserId, status);
        }

        public async Task<List<OrderHistory>> GetHistoriesAsync(long orderId)
        {
            return await _orderHistoryRepo.GetByOrderIdAsync(orderId);
        }

        public async Task<Order?> GetByIdAsync(long orderId)
        {
            return await _orderRepo.GetByIdAsync(orderId);
        }
    }
}
