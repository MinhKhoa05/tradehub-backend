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

        public OrderService(IOrderRepository orderRepo, IOrderHistoryRepository orderHistoryRepo)
        {
            _orderRepo = orderRepo;
            _orderHistoryRepo = orderHistoryRepo;
        }

        public async Task<bool> HasPendingOrderAsync(long userId)
        {
            return await _orderRepo.HasPendingOrderAsync(userId);
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

        public async Task<Order> LockAndGetByIdAsync(long orderId)
        {
            var order = await _orderRepo.GetByIdForUpdateAsync(orderId)
                ?? throw new NotFoundException($"Không tìm thấy đơn hàng #{orderId}");
            return order;
        }

        public async Task<long> CreateOrderAsync(Order order, UserContext user)
        {
            if (await HasPendingOrderAsync(user.UserId))
            {
                throw new BusinessException("Bạn đang có một đơn hàng chờ thanh toán. Vui lòng hoàn tất hoặc hủy đơn hàng đó trước khi tạo đơn mới.");
            }

            var newOrderId = await _orderRepo.CreateAsync(order);
            order.Id = newOrderId;

            // WHY: Lưu trữ log lịch sử ngay khi khởi tạo đơn hàng để dễ tracking dòng đời Order.
            await _orderHistoryRepo.CreateAsync(new OrderHistory
            {
                OrderId = newOrderId,
                FromStatus = order.Status,
                ToStatus = order.Status,
                Note = "Đơn hàng được tạo (Chờ thanh toán).",
                ActionBy = user.UserId,
                IsAdmin = false,
                CreatedAt = DateTime.UtcNow
            });

            return newOrderId;
        }

        public async Task PickOrderAsync(Order order, UserContext admin)
        {
            // WHY: Tránh lỗi giao diện khi Admin bấm "Tiếp nhận" liên tiếp nhiều lần (Idempotent).
            if (order.Status == OrderStatus.Processing && order.AssignTo == admin.UserId)
                return;

            // WHY: Chỉ cho phép Admin nhận các đơn hàng ĐÃ THANH TOÁN (Paid).
            if (order.Status != OrderStatus.Paid)
                throw new BusinessException("Chỉ có thể tiếp nhận đơn hàng đã thanh toán.");

            var fromStatus = order.Status;

            order.Status = OrderStatus.Processing;
            order.AssignTo = admin.UserId;
            order.AssignAt = DateTime.UtcNow;
            order.UpdatedAt = DateTime.UtcNow;

            await _orderRepo.UpdateAsync(order);

            await _orderHistoryRepo.CreateAsync(new OrderHistory
            {
                OrderId = order.Id,
                FromStatus = fromStatus,
                ToStatus = OrderStatus.Processing,
                Note = $"Admin {admin.Username} tiếp nhận đơn hàng.",
                ActionBy = admin.UserId,
                IsAdmin = true,
                CreatedAt = DateTime.UtcNow
            });
        }

        public async Task CompleteOrderAsync(Order order, UserContext admin)
        {
            // WHY: Hỗ trợ retry an toàn nếu mạng chập chờn khi Admin bấm Hoàn thành.
            if (order.Status == OrderStatus.Completed)
                return;

            // WHY: Bắt buộc đơn hàng phải trải qua bước Processing trước khi Hoàn thành.
            if (order.Status != OrderStatus.Processing)
                throw new BusinessException("Trạng thái đơn hàng không hợp lệ để hoàn thành.");

            // WHY: Đảm bảo tính trách nhiệm. Admin nào tiếp nhận (Pick) thì Admin đó mới có quyền Hoàn thành.
            if (order.AssignTo != admin.UserId)
                throw new BusinessException("Bạn không thể can thiệp vào đơn hàng của người khác.");

            var fromStatus = order.Status;

            order.Status = OrderStatus.Completed;
            order.UpdatedAt = DateTime.UtcNow;

            await _orderRepo.UpdateAsync(order);

            await _orderHistoryRepo.CreateAsync(new OrderHistory
            {
                OrderId = order.Id,
                FromStatus = fromStatus,
                ToStatus = OrderStatus.Completed,
                Note = $"Admin {admin.Username} xác nhận hoàn thành.",
                ActionBy = admin.UserId,
                IsAdmin = true,
                CreatedAt = DateTime.UtcNow
            });
        }

        public async Task<OrderStatus?> CancelOrderAsync(Order order, UserContext admin, string? reason = null)
        {
            // WHY: Trả về trạng thái cũ để UseCase biết có cần hoàn tiền/hoàn kho hay không.
            if (order.Status == OrderStatus.Cancelled || order.Status == OrderStatus.Completed)
                return null;

            var oldStatus = order.Status;

            order.Status = OrderStatus.Cancelled;
            order.UpdatedAt = DateTime.UtcNow;

            await _orderRepo.UpdateAsync(order);

            var note = $"Hủy đơn hàng." + (string.IsNullOrEmpty(reason) ? "" : $" Lý do: {reason}");

            await _orderHistoryRepo.CreateAsync(new OrderHistory
            {
                OrderId = order.Id,
                FromStatus = oldStatus,
                ToStatus = OrderStatus.Cancelled,
                Note = note,
                ActionBy = admin.UserId,
                CreatedAt = DateTime.UtcNow
            });

            return oldStatus;
        }

        public async Task PayOrderAsync(Order order, UserContext user, string note)
        {
            var fromStatus = OrderStatus.Pending; 
            order.Status = OrderStatus.Paid; 
            order.UpdatedAt = DateTime.UtcNow;
            
            await _orderRepo.UpdateAsync(order);

            await _orderHistoryRepo.CreateAsync(new OrderHistory
            {
                OrderId = order.Id,
                FromStatus = fromStatus, 
                ToStatus = order.Status,
                Note = note,
                ActionBy = user.UserId,
                IsAdmin = false,
                CreatedAt = DateTime.UtcNow
            });
        }
    }
}
