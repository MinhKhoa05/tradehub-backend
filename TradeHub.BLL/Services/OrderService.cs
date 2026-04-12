using TradeHub.BLL.Common;
using TradeHub.BLL.DTOs.Orders;
using TradeHub.BLL.Exceptions;
using TradeHub.DAL;
using TradeHub.DAL.Entities;
using TradeHub.DAL.Repositories;

namespace TradeHub.BLL.Services
{
    public class OrderService : BaseService
    {
        private readonly OrderRepository _orderRepo;
        private readonly OrderItemRepository _orderItemRepo;
        private readonly OrderHistoryRepository _orderHistoryRepo;
        private readonly DatabaseContext _database;

        public OrderService(OrderRepository orderRepo, OrderItemRepository orderItemRepo,
                OrderHistoryRepository orderHistoryRepo, DatabaseContext database, IIdentityService identityService)
            : base(identityService)
        {
            _orderRepo = orderRepo;
            _orderItemRepo = orderItemRepo;
            _orderHistoryRepo = orderHistoryRepo;
            _database = database;
        }

        // ===== READ (Giữ 'My' để phân biệt phạm vi Người mua/Người bán) =====

        public async Task<List<Order>> GetMyOrdersAsync(OrderType type)
        {
            return type switch
            {
                OrderType.Seller => await _orderRepo.GetSellerOrdersAsync(CurrentUserId),
                OrderType.Buyer => await _orderRepo.GetBuyerOrdersAsync(CurrentUserId),
                _ => await _orderRepo.GetAllByUserId(CurrentUserId)
            };
        }

        public async Task<List<OrderHistory>> GetHistoriesAsync(int orderId)
        {
            // Bỏ 'Order' trong tên hàm vì đang ở OrderService
            await EnsureOrderBelongsToMeAsync(orderId);
            return await _orderHistoryRepo.GetByOrderIdAsync(orderId);
        }

        public async Task<List<OrderItem>> GetItemsAsync(int orderId)
        {
            await EnsureOrderBelongsToMeAsync(orderId);
            return await _orderItemRepo.GetByOrderIdAsync(orderId);
        }

        // ===== ACTIONS (Thao tác nghiệp vụ - Bỏ 'My') =====

        public async Task<List<long>> CreateOrdersAsync(CheckoutRequest request)
        {
            if (request.Items == null || !request.Items.Any())
                throw new BusinessException("Đơn hàng phải có ít nhất 1 sản phẩm.");

            // Gom nhóm sản phẩm theo người bán để tách thành các đơn hàng riêng biệt
            var groups = request.Items.GroupBy(x => x.SellerId);
            var orderIds = new List<long>();

            foreach (var group in groups)
            {
                var orderId = await CreateOrderInternalAsync(
                    group.Key,
                    request.PaymentMethod,
                    group.ToList());

                orderIds.Add(orderId);
            }

            return orderIds;
        }

        public async Task UpdateStatusAsync(int orderId, UpdateOrderStatusRequest request)
        {
            var order = await _orderRepo.GetByIdAsync(orderId)
                            ?? throw new BusinessException("Đơn hàng không tồn tại.");

            // Chốt chặn bảo mật quan trọng nhất
            await EnsureOrderBelongsToMeAsync(orderId);

            var currentStatus = order.Status;

            if (!IsValidStatusTransition(currentStatus, request.ToStatus))
                throw new BusinessException($"Không thể chuyển trạng thái từ {currentStatus} sang {request.ToStatus}.");

            await _database.ExecuteInTransactionAsync(async () =>
            {
                var affectedRows = await _orderRepo.UpdateStatusAsync(orderId, request.ToStatus);

                if (affectedRows == 0)
                    throw new BusinessException("Cập nhật trạng thái thất bại.");

                await _orderHistoryRepo.CreateAsync(new OrderHistory
                {
                    OrderId = orderId,
                    FromStatus = currentStatus,
                    ToStatus = request.ToStatus,
                    ChangedBy = CurrentUserId,
                    ActorType = request.ActorType,
                    Note = request.Note
                });
            });
        }

        // ================= INTERNAL / PRIVATE (Hậu tố Internal & Ensure) =================

        private async Task EnsureOrderBelongsToMeAsync(long orderId)
        {
            // Kiểm tra xem User hiện tại có phải Buyer hoặc Seller của đơn này không
            var isBelong = await _orderRepo.IsOrderBelongsToUserAsync(CurrentUserId, orderId);

            if (!isBelong)
                throw new BusinessException("Bạn không có quyền truy cập vào đơn hàng này.");
        }

        private async Task<long> CreateOrderInternalAsync(
            int sellerId,
            PaymentMethod paymentMethod,
            List<CheckoutItem> items)
        {
            var order = new Order
            {
                BuyerId = CurrentUserId,
                SellerId = sellerId,
                PaymentMethod = paymentMethod,
                Status = OrderStatus.Pending,
                TotalAmount = CalculateTotalInternal(items)
            };

            var orderId = await _orderRepo.CreateAsync(order);

            // Tách nhỏ các bước tạo Item và History
            await CreateOrderItemsInternalAsync(orderId, items);
            await CreateInitialHistoryInternalAsync(orderId);

            return orderId;
        }

        private async Task CreateOrderItemsInternalAsync(long orderId, List<CheckoutItem> items)
        {
            var orderItems = items.Select(x => new OrderItem
            {
                OrderId = orderId,
                ProductId = x.ProductId,
                Quantity = x.Quantity,
                UnitPrice = x.UnitPrice
            }).ToList();

            await _orderItemRepo.CreateRangeAsync(orderItems);
        }

        private async Task CreateInitialHistoryInternalAsync(long orderId)
        {
            await _orderHistoryRepo.CreateAsync(new OrderHistory
            {
                OrderId = orderId,
                ToStatus = OrderStatus.Pending,
                ChangedBy = CurrentUserId,
                ActorType = ActorType.Buyer,
                Note = "Đã đặt hàng thành công."
            });
        }

        private static int CalculateTotalInternal(List<CheckoutItem> items)
        {
            return items.Sum(i => i.Quantity * i.UnitPrice);
        }

        private static bool IsValidStatusTransition(OrderStatus current, OrderStatus next)
        {
            // Giữ nguyên logic State Machine của mày (Rất tốt!)
            return (current, next) switch
            {
                (OrderStatus.Pending, OrderStatus.Confirmed) => true,
                (OrderStatus.Pending, OrderStatus.Cancelled) => true,
                (OrderStatus.Confirmed, OrderStatus.ReadyForPickup) => true,
                (OrderStatus.Confirmed, OrderStatus.Cancelled) => true,
                (OrderStatus.ReadyForPickup, OrderStatus.Delivering) => true,
                (OrderStatus.Delivering, OrderStatus.Delivered) => true,
                (OrderStatus.Delivering, OrderStatus.DeliveryFailed) => true,
                (OrderStatus.Delivered, OrderStatus.Completed) => true,
                _ => false
            };
        }
    }
}