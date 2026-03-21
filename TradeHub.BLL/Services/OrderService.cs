using TradeHub.BLL.DTOs.Orders;
using TradeHub.BLL.Exceptions;
using TradeHub.DAL;
using TradeHub.DAL.Entities;
using TradeHub.DAL.Repositories;

namespace TradeHub.BLL.Services
{
    public class OrderService
    {
        private readonly OrderRepository _orderRepo;
        private readonly OrderItemRepository _orderItemRepo;
        private readonly OrderHistoryRepository _orderHistoryRepo;
        private readonly DatabaseContext _database;
        
        public OrderService(OrderRepository orderRepo, OrderItemRepository orderItemRepo,
            OrderHistoryRepository orderHistoryRepo, DatabaseContext database)
        { 
            _orderRepo = orderRepo;
            _orderItemRepo = orderItemRepo;
            _orderHistoryRepo = orderHistoryRepo;
            _database = database;
        }

        public async Task<List<Order>> GetOrdersByIdAsync(int userId, OrderType type)
        {
            switch (type)
            {
                case OrderType.Seller:
                    return await _orderRepo.GetSellerOrdersAsync(userId);

                case OrderType.Buyer:
                    return await _orderRepo.GetBuyerOrdersAsync(userId);

                default:
                    return await _orderRepo.GetAllByUserId(userId);
            }
        }

        public async Task<List<OrderHistory>> GetOrderHistoriesAsync(int userId, int orderId)
        {
            var isBelongToUser = await _orderRepo.IsOrderBelongsToUserAsync(userId, orderId);
            if (!isBelongToUser)
            {
                throw new BusinessException("Đơn hàng không tồn tại hoặc bạn không có quyền chi tiết đơn hàng này");
            }
            
            return await _orderHistoryRepo.GetByOrderIdAsync(orderId);
        }

        public async Task<List<OrderItem>> GetOrderItemsAsync(int userId, int orderId)
        {
            var isBelongToUser = await _orderRepo.IsOrderBelongsToUserAsync(userId, orderId);
            if (!isBelongToUser)
            {
                throw new BusinessException("Đơn hàng không tồn tại hoặc bạn không có quyền chi tiết đơn hàng này");
            }

            return await _orderItemRepo.GetByOrderIdAsync(orderId);
        }

        public async Task<List<int>> CreateOrdersAsync(int userId, CheckoutRequest request)
        {
            if (request.Items == null || request.Items.Count == 0)
                throw new BusinessException("Đơn hàng phải có ít nhất 1 sản phẩm");

            var groups = request.Items.GroupBy(x => x.SellerId);

            var orderIds = new List<int>();
            
            // Nhóm sản phẩm theo seller
            foreach (var group in groups)
            {
                var orderId = await CreateOrderForSellerAsync(userId, group.Key, request.PaymentMethod, group.ToList());

                orderIds.Add(orderId);
            }

            return orderIds;
        }

        public async Task UpdateStatusAsync(int userId, int orderId, UpdateOrderStatusRequest request)
        {
            var order = await _orderRepo.GetByIdAsync(orderId)
                            ?? throw new BusinessException("Đơn hàng không tồn tại");

            var currentStatus = order.Status;

            if (!IsValidStatusTransition(currentStatus, request.ToStatus))
                throw new BusinessException($"Chuyển đổi status không khả dụng: {currentStatus} -> {request.ToStatus}");

            await _database.ExecuteInTransactionAsync(async () =>
            {
                var affectedRows = await _orderRepo.UpdateStatusAsync(orderId, request.ToStatus);

                if (affectedRows == 0)
                    throw new BusinessException("Đơn hàng không tồn tại");

                await _orderHistoryRepo.CreateAsync(new OrderHistory
                {
                    OrderId = orderId,
                    FromStatus = currentStatus,
                    ToStatus = request.ToStatus,
                    ChangedBy = userId,
                    ActorType = request.ActorType,
                    Note = request.Note
                });
            });
        }

        private async Task<int> CreateOrderForSellerAsync(int userId, int sellerId, PaymentMethod paymentMethod, List<CheckoutItem> items)
        {
            var order = new Order
            {
                BuyerId = userId,
                SellerId = sellerId,
                PaymentMethod = paymentMethod,
                Status = OrderStatus.Pending,
                TotalAmount = CalculateOrderTotal(items)
            };

            var orderId = await _orderRepo.CreateAsync(order);

            await CreateOrderItemsAsync(orderId, items);

            await CreateOrderHistoryAsync(userId, orderId);

            return orderId;
        }

        private async Task CreateOrderItemsAsync(int orderId, List<CheckoutItem> items)
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

        private async Task CreateOrderHistoryAsync(int userId, int orderId)
        {
            var history = new OrderHistory
            {
                OrderId = orderId,
                ToStatus = OrderStatus.Pending,
                ChangedBy = userId,
                ActorType = ActorType.Buyer,
                Note = "Đặt hàng thành công, đang chờ người bán xác nhận"
            };

            await _orderHistoryRepo.CreateAsync(history);
        }

        private static int CalculateOrderTotal(List<CheckoutItem> items)
        {
            return items.Sum(i => i.Quantity * i.UnitPrice);
        }

        private static bool IsValidStatusTransition(OrderStatus current, OrderStatus next)
        {
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