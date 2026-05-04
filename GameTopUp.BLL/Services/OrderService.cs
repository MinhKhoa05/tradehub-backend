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

        public async Task<long> CreateOrder(Order order, UserContext user)
        {
            var newOrderId = await _orderRepo.CreateAsync(order);
            order.Id = newOrderId;

            // WHY: Lưu trữ log lịch sử ngay khi khởi tạo đơn hàng để dễ tracking dòng đời Order.
            await _orderHistoryRepo.CreateAsync(new OrderHistory
            {
                OrderId = newOrderId,
                FromStatus = OrderStatus.Pending,
                ToStatus = OrderStatus.Pending,
                Note = "Đơn hàng được tạo.",
                ActionBy = user.UserId,
                IsAdmin = false,
                CreatedAt = DateTime.UtcNow
            });

            return newOrderId;
        }

        public async Task PickOrder(Order order, UserContext admin)
        {
            // WHY: Tránh lỗi giao diện khi Admin bấm "Tiếp nhận" liên tiếp nhiều lần (Idempotent).
            if (order.Status == OrderStatus.Processing && order.AssignTo == admin.UserId)
                return;

            // WHY: Đơn hàng phải ở trạng thái chờ mới được xử lý. Tránh việc đơn đã Hủy hoặc Hoàn thành bị can thiệp.
            if (order.Status != OrderStatus.Pending)
                throw new BusinessException("Đơn hàng không còn ở trạng thái chờ.");

            // WHY: Ngăn chặn tình trạng 2 Admin cùng lúc nhảy vào xử lý chung một đơn hàng (Race Condition).
            if (order.AssignTo != 0)
                throw new BusinessException("Đơn hàng đã được tiếp nhận bởi người khác.");

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

        public async Task CompleteOrder(Order order, UserContext admin)
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

        public async Task<bool> CancelOrder(Order order, UserContext admin, string? reason = null)
        {
            // WHY: Trả về bool (isNewCancellation) để UseCase biết có cần thực hiện Hoàn tiền hay không, 
            // tránh lỗi hoàn tiền 2 lần cho khách hàng nếu có request hủy trùng lặp.
            if (order.Status == OrderStatus.Cancelled)
                return false;

            // WHY: Chỉ có thể hủy khi đơn chưa được xử lý. Nếu đang Processing thì phải Release trước (nếu có nghiệp vụ).
            if (order.Status != OrderStatus.Pending)
                throw new BusinessException("Không thể hủy đơn hàng đang ở trạng thái này.");

            var fromStatus = order.Status;

            order.Status = OrderStatus.Cancelled;
            order.UpdatedAt = DateTime.UtcNow;

            await _orderRepo.UpdateAsync(order);

            var note = $"Admin {admin.Username} hủy đơn hàng." + (string.IsNullOrEmpty(reason) ? "" : $" Lý do: {reason}");

            await _orderHistoryRepo.CreateAsync(new OrderHistory
            {
                OrderId = order.Id,
                FromStatus = fromStatus,
                ToStatus = OrderStatus.Cancelled,
                Note = note,
                ActionBy = admin.UserId,
                IsAdmin = true,
                CreatedAt = DateTime.UtcNow
            });

            return true;
        }

        public async Task AddHistoryAsync(Order order, OrderStatus fromStatus, UserContext admin, string note)
        {
            await _orderHistoryRepo.CreateAsync(new OrderHistory
            {
                OrderId = order.Id,
                FromStatus = fromStatus,
                ToStatus = order.Status,
                Note = note,
                ActionBy = admin.UserId,
                IsAdmin = true,
                CreatedAt = DateTime.UtcNow
            });
        }
    }
}
