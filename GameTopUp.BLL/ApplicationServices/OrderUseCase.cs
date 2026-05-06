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
            // WHY: Đưa logic kiểm tra nghiệp vụ xuống Service.
            await _packageService.CheckAvailabilityAsync(request.GamePackageId, request.Quantity);
            
            var package = await _packageService.GetPackageByIdAsync(request.GamePackageId);

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
            await _database.ExecuteInTransactionAsync(async () =>
            {
                // 1. Khóa và lấy thông tin đơn hàng
                var order = await _orderService.LockAndGetByIdAsync(orderId);
                
                // 2. Validate nghiệp vụ (ownership, status)
                _orderService.ValidateForPayment(order, context);

                // 3. Khóa ví và trừ tiền
                // WHY: Phải trừ tiền thành công trước khi cập nhật trạng thái đơn hàng.
                var wallet = await _walletService.LockAndGetByUserIdAsync(context.UserId);
                await _walletService.DebitAsync(
                    wallet, 
                    order.Total, 
                    WalletTransactionType.PaidOrder, 
                    $"Thanh toán đơn hàng #{order.Id}", 
                    order.Id);

                // 4. Cập nhật trạng thái đơn hàng sang Paid
                await _orderService.MarkAsPaidAsync(order, context);
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

                // 2. Đơn hàng đã xử lý trước đó rồi thì không cần làm gì nữa
                if (oldStatus == null) return;

                // 3. HOÀN KHO: Vì khi đặt hàng (PlaceOrder) chúng ta đã trừ tồn kho
                await _packageService.IncreaseStockAsync(order.GamePackageId, order.Quantity);

                // 4. HOÀN TIỀN: Chỉ hoàn tiền nếu đơn hàng đã ở trạng thái Paid hoặc Processing
                if (oldStatus == OrderStatus.Paid || oldStatus == OrderStatus.Processing)
                {
                    // WHY: Khóa ví trước khi hoàn tiền để tránh Race Condition (Pessimistic Locking).
                    var wallet = await _walletService.LockAndGetByUserIdAsync(order.UserId);
                    await _walletService.CreditAsync(wallet, order.Total, WalletTransactionType.Refund, $"Hoàn tiền đơn hàng #{orderId}. {reason ?? ""}", orderId);
                }
            });
        }
    }
}
