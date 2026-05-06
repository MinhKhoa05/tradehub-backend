using GameTopUp.BLL.Common;
using GameTopUp.BLL.Services;
using GameTopUp.BLL.Exceptions;
using GameTopUp.DAL;
using GameTopUp.DAL.Entities;
using GameTopUp.BLL.DTOs.Orders;

namespace GameTopUp.BLL.ApplicationServices
{
    public class OrderUseCase
    {
        private readonly GamePackageService _packageService;
        private readonly WalletService _walletService;
        private readonly OrderService _orderService;
        private readonly DatabaseContext _database;

        public OrderUseCase(
            GamePackageService packageService,
            WalletService walletService,
            OrderService orderService,
            DatabaseContext database)
        {
            _packageService = packageService;
            _walletService = walletService;
            _orderService = orderService;
            _database = database;
        }

        public async Task<long> PlaceOrderAsync(UserContext context, PlaceOrderRequestDTO request)
        {
            var package = await _packageService.GetPackageByIdAsync(request.GamePackageId);
            if (!package.IsActive) throw new BusinessException("Gói nạp hiện không khả dụng.");
            if (package.StockQuantity < request.Quantity) throw new BusinessException("Số lượng trong kho không đủ.");
            
            return await _database.ExecuteInTransactionAsync(async () =>
            {
                // WHY: Trừ tồn kho ngay khi đặt hàng để đảm bảo "giữ chỗ" sản phẩm cho khách.
                await _packageService.DecreaseStockAsync(request.GamePackageId, request.Quantity);
                
                var order = new Order
                {
                    UserId = context.UserId,
                    GamePackageId = request.GamePackageId,
                    UnitPrice = package.SalePrice,
                    Quantity = request.Quantity,
                    GameAccountInfo = request.GameAccountInfo,
                    Status = OrderStatus.Pending,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                return await _orderService.CreateOrderAsync(order, context);
            });            
        }

        public async Task PayOrderAsync(long orderId, UserContext context)
        {
            await _database.ExecuteInTransactionAsync(async () => {

                var order = await _orderService.LockAndGetByIdAsync(orderId);
                
                if (order.UserId != context.UserId) throw new BusinessException("Bạn không có quyền thanh toán đơn hàng này.");
                if (order.Status != OrderStatus.Pending) throw new BusinessException("Đơn hàng không ở trạng thái chờ thanh toán.");
            
                // 1. Trừ tiền ví và gắn kết với OrderId
                await _walletService.DeductMoneyAsync(context, order.Total, $"Thanh toán đơn hàng #{order.Id}", order.Id);
                
                // 2. Gọi Service xử lý nghiệp vụ PayOrder (Cập nhật trạng thái & Lịch sử)
                await _orderService.PayOrderAsync(order, context, "Thanh toán đơn hàng thành công.");
            });
        }

        public async Task PickOrderAsync(long orderId, UserContext adminContext)
        {
            await _database.ExecuteInTransactionAsync(async () =>
            {
                var order = await _orderService.LockAndGetByIdAsync(orderId);
                await _orderService.PickOrderAsync(order, adminContext);
            });
        }

        public async Task CompleteOrderAsync(long orderId, UserContext adminContext)
        {
            await _database.ExecuteInTransactionAsync(async () =>
            {
                var order = await _orderService.LockAndGetByIdAsync(orderId);
                await _orderService.CompleteOrderAsync(order, adminContext);
            });
        }

        public async Task CancelOrderAsync(long orderId, UserContext adminContext, string? reason = null)
        {
            await _database.ExecuteInTransactionAsync(async () =>
            {
                var order = await _orderService.LockAndGetByIdAsync(orderId);

                // 1. Thực hiện hủy đơn trong Service để lấy trạng thái cũ
                var oldStatus = await _orderService.CancelOrderAsync(order, adminContext, reason);

                if (oldStatus != null)
                {
                    // 2. HOÀN KHO: Vì khi đặt hàng (PlaceOrder) chúng ta đã trừ tồn kho ngay
                    await _packageService.IncreaseStockAsync(order.GamePackageId, order.Quantity);

                    // 3. HOÀN TIỀN: Chỉ hoàn tiền nếu đơn hàng đã ở trạng thái Paid hoặc Processing
                    if (oldStatus == OrderStatus.Paid || oldStatus == OrderStatus.Processing)
                    {
                        var userContext = new UserContext { UserId = order.UserId };
                        await _walletService.RefundMoneyAsync(userContext, order.Total, $"Hoàn tiền đơn hàng #{orderId}. {reason ?? ""}", orderId);
                    }
                }
            });
        }
    }
}
