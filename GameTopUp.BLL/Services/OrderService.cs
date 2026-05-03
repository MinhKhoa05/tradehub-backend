using GameTopUp.DAL;
using GameTopUp.DAL.Entities;
using GameTopUp.DAL.Interfaces;
using GameTopUp.BLL.Common;
using GameTopUp.BLL.Exceptions;

namespace GameTopUp.BLL.Services
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

        public async Task PickOrderAsync(long orderId, UserContext context)
        {
            // 1. Kiểm tra quyền hạn (Thực tế thường check ở Controller nhưng check lại ở đây cho chắc)
            if (context.Role != "Admin")
            {
                throw new ForbiddenException("Chỉ Admin mới có quyền nhận xử lý đơn hàng.");
            }

            // 2. Thực thi lệnh cập nhật mang tính Atomic (Nguyên tử)
            await _database.ExecuteInTransactionAsync(async () =>
            {
                var affectedRows = await _orderRepo.PickOrderAsync(orderId, context.UserId);
                
                if (affectedRows == 0)
                {
                    // Nếu không có dòng nào bị ảnh hưởng, có 2 khả năng:
                    // - Đơn hàng không tồn tại
                    // - Đơn hàng đã được Admin khác nhận rồi (Race condition)
                    var order = await _orderRepo.GetByIdAsync(orderId);
                    if (order == null) throw new NotFoundException($"Không tìm thấy đơn hàng #{orderId}");
                    
                    throw new BusinessException("Đơn hàng này đã được người khác tiếp nhận hoặc không còn ở trạng thái chờ.");
                }

                // 3. Ghi log lịch sử trạng thái
                await _orderHistoryRepo.CreateAsync(new OrderHistory
                {
                    OrderId = orderId,
                    FromStatus = OrderStatus.Pending,
                    ToStatus = OrderStatus.Processing,
                    Note = $"Admin {context.Username} đã tiếp nhận đơn hàng.",
                    ActionBy = context.UserId,
                    IsAdmin = true,
                    CreatedAt = DateTime.UtcNow
                });
            });
        }

        public async Task<bool> CancelOrderAsync(long orderId, UserContext adminContext)
        {
            var affectedRows = await _orderRepo.CancelOrderAsync(orderId);
            
            if (affectedRows == 0)
            {
                // Rationale: Nếu không cập nhật được, có thể do đơn hàng đã đổi trạng thái (Race Condition)
                // Ta không ném lỗi ở đây mà để UseCase quyết định tính Idempotency.
                var order = await _orderRepo.GetByIdAsync(orderId);
                if (order == null) throw new NotFoundException($"Không tìm thấy đơn hàng #{orderId}");
                
                return false;
            }

            // Ghi log lịch sử trạng thái
            await _orderHistoryRepo.CreateAsync(new OrderHistory
            {
                OrderId = orderId,
                FromStatus = OrderStatus.Pending,
                ToStatus = OrderStatus.Cancelled,
                Note = $"Admin {adminContext.Username} đã hủy đơn hàng và hoàn tiền.",
                ActionBy = adminContext.UserId,
                IsAdmin = true,
                CreatedAt = DateTime.UtcNow
            });

            return true;
        }

        public async Task CompleteOrderAsync(long orderId, UserContext context)
        {
            // 1. Kiểm tra quyền hạn (Fail Fast)
            if (context.Role != "Admin")
            {
                throw new ForbiddenException("Chỉ Admin mới có quyền hoàn thành đơn hàng.");
            }

            // 2. Kiểm tra sự tồn tại và trạng thái hiện tại
            var order = await _orderRepo.GetByIdAsync(orderId)
                ?? throw new NotFoundException($"Không tìm thấy đơn hàng #{orderId}");

            // 3. Tính Idempotency (Giao hoán): Nếu đơn đã hoàn thành rồi thì trả về thành công luôn
            if (order.Status == OrderStatus.Completed)
            {
                return;
            }

            // 4. Ràng buộc nghiệp vụ: 
            // - Phải ở trạng thái Processing
            // - Phải đúng Admin đang giữ đơn
            if (order.Status != OrderStatus.Processing)
            {
                throw new BusinessException($"Chỉ có thể hoàn thành đơn hàng đang ở trạng thái Đang xử lý (Processing). Trạng thái hiện tại: {order.Status}");
            }

            if (order.AssignTo != context.UserId)
            {
                throw new BusinessException("Bạn không thể hoàn thành đơn hàng này vì nó đang được xử lý bởi một Admin khác.");
            }

            // 5. Thực thi cập nhật trạng thái
            await _database.ExecuteInTransactionAsync(async () =>
            {
                var affectedRows = await _orderRepo.CompleteOrderAsync(orderId, context.UserId);

                if (affectedRows == 0)
                {
                    // Race condition check: Nếu không update được, kiểm tra xem có phải do Admin khác đã xử lý hoặc đơn đã đổi trạng thái
                    var currentOrder = await _orderRepo.GetByIdAsync(orderId);
                    if (currentOrder?.Status == OrderStatus.Completed)
                    {
                        return; // Idempotent success
                    }

                    throw new BusinessException("Không thể cập nhật trạng thái đơn hàng. Vui lòng kiểm tra lại trạng thái hoặc quyền sở hữu đơn hàng.");
                }

                // 6. Ghi log lịch sử trạng thái
                await _orderHistoryRepo.CreateAsync(new OrderHistory
                {
                    OrderId = orderId,
                    FromStatus = OrderStatus.Processing,
                    ToStatus = OrderStatus.Completed,
                    Note = $"Admin {context.Username} đã xác nhận hoàn thành đơn hàng.",
                    ActionBy = context.UserId,
                    IsAdmin = true,
                    CreatedAt = DateTime.UtcNow
                });
            });
        }
    }
}
