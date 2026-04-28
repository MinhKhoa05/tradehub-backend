using TradeHub.BLL.Common;
using TradeHub.BLL.Services;
using TradeHub.DAL;
using TradeHub.DAL.Entities;

namespace TradeHub.BLL.ApplicationServices
{
    public class OrderUseCase : BaseService
    {
        private readonly CartService _cartService;
        private readonly WalletService _walletService;
        private readonly OrderService _orderService;
        private readonly DatabaseContext _database;

        public OrderUseCase(
            CartService cartService,
            WalletService walletService,
            OrderService orderService,
            DatabaseContext database,
            IIdentityService identityService)
            : base(identityService)
        {
            _cartService = cartService;
            _walletService = walletService;
            _orderService = orderService;
            _database = database;
        }

        /// <summary>
        /// Thực hiện quy trình Checkout
        /// </summary>
        /// <param name="userId">ID người dùng</param>
        /// <param name="cartId">ID giỏ hàng (Trong logic hiện tại là UserId)</param>
        /// <param name="gameAccountInfo">Thông tin tài khoản game để nạp</param>
        /// <returns>Kết quả xử lý Checkout</returns>
        public async Task<ServiceResult> CheckoutAsync(long userId, long cartId, string gameAccountInfo)
        {
            try
            {
                // 1. Lấy thông tin giỏ hàng
                var cartItems = await _cartService.GetMyDetailsAsync();
                if (cartItems == null || !cartItems.Any())
                {
                    return ServiceResult.Failure("Giỏ hàng của bạn đang trống.");
                }

                // 2. Tính tổng tiền
                decimal totalAmount = cartItems.Sum(item => item.Price * item.Quantity);

                // 3. Sử dụng Transaction để đảm bảo tính toàn vẹn (Atomic: Trừ tiền + Tạo đơn + Xóa giỏ)
                return await _database.ExecuteInTransactionAsync(async () =>
                {
                    // A. Trừ tiền từ ví
                    // Lưu ý: Nếu DeductMoneyAsync ném Exception, Transaction sẽ tự động Rollback
                    long transactionId = await _walletService.DeductMoneyAsync(userId, totalAmount, $"Thanh toán đơn hàng từ giỏ hàng.");

                    var orderIds = new List<long>();

                    // B. Tạo đơn hàng cho từng sản phẩm trong giỏ (Hoặc gộp lại tùy nghiệp vụ)
                    // Ở đây tôi tạo mỗi item là một Order theo cấu trúc bảng Orders hiện tại
                    foreach (var item in cartItems)
                    {
                        var order = new Order
                        {
                            UserId = userId,
                            GamePackageId = item.ProductId,
                            UnitPrice = item.Price,
                            Quantity = item.Quantity,
                            GameAccountInfo = gameAccountInfo,
                            Status = OrderStatus.Pending
                        };

                        var orderId = await _orderService.CreateOrderAsync(order, transactionId);
                        orderIds.Add(orderId);
                    }

                    // C. Xóa giỏ hàng sau khi đặt thành công
                    await _cartService.ClearAsync();

                    return ServiceResult.Success("Đặt hàng thành công!", new { OrderIds = orderIds, TransactionId = transactionId });
                });
            }
            catch (Exception ex)
            {
                // Xử lý ngoại lệ chặt chẽ: lỗi số dư không đủ, lỗi kết nối DB, v.v.
                // Nếu là lỗi nghiệp vụ (BusinessException), ta trả về message cụ thể
                return ServiceResult.Failure($"Lỗi Checkout: {ex.Message}");
            }
        }
    }
}
