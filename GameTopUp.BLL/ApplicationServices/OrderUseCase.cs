using GameTopUp.BLL.Common;
using GameTopUp.BLL.Services;
using GameTopUp.BLL.Exceptions;
using GameTopUp.DAL;
using GameTopUp.DAL.Entities;
using GameTopUp.DAL.DTOs;
using GameTopUp.BLL.DTOs.Orders;

namespace GameTopUp.BLL.ApplicationServices
{
    /// <summary>
    /// OrderUseCase điều phối luồng nghiệp vụ phức tạp liên quan đến đặt hàng.
    /// UseCase đóng vai trò là "nhạc trưởng", kết hợp nhiều Service (Cart, Wallet, Order) 
    /// trong một Transaction duy nhất để đảm bảo tính toàn vẹn dữ liệu.
    /// </summary>
    public class OrderUseCase
    {
        private readonly CartService _cartService;
        private readonly WalletService _walletService;
        private readonly OrderService _orderService;
        private readonly DatabaseContext _database;

        public OrderUseCase(
            CartService cartService,
            WalletService walletService,
            OrderService orderService,
            DatabaseContext database)
        {
            _cartService = cartService;
            _walletService = walletService;
            _orderService = orderService;
            _database = database;
        }

        /// <summary>
        /// Luồng Checkout: Kiểm tra giỏ -> Trừ tiền ví -> Tạo đơn hàng -> Xóa giỏ.
        /// Sử dụng Transaction để đảm bảo nếu bất kỳ bước nào thất bại, tiền của khách hàng sẽ không bị mất
        /// và giỏ hàng không bị xóa oan.
        /// </summary>
        public async Task<CheckoutResponseDTO> CheckoutAsync(UserContext context, string gameAccountInfo)
        {
            var cartItems = await _cartService.GetDetailsAsync(context);
            if (cartItems == null || !cartItems.Any())
            {
                throw new BusinessException("Giỏ hàng của bạn đang trống.");
            }

            decimal totalAmount = cartItems.Sum(item => item.Price * item.Quantity);

            // Thực hiện toàn bộ logic trong Transaction để đảm bảo tính Atomicity (Tất cả hoặc không gì cả)
            return await _database.ExecuteInTransactionAsync(async () =>
            {
                // 1. Trừ tiền trước để xác thực khả năng thanh toán
                var txResult = await _walletService.DeductMoneyAsync(context, totalAmount, "Thanh toán đơn hàng từ giỏ hàng.");
                
                // 2. Tạo các bản ghi đơn hàng tương ứng
                var orderIds = await CreateOrdersFromCartAsync(context, cartItems, gameAccountInfo, txResult.TransactionId);
                
                // 3. Xóa giỏ hàng sau khi đã đặt hàng thành công
                await _cartService.ClearAsync(context);

                return new CheckoutResponseDTO { OrderIds = orderIds, TransactionId = txResult.TransactionId };
            });
        }

        /// <summary>
        /// Tách nhỏ logic tạo đơn hàng để hàm chính (CheckoutAsync) tập trung vào luồng thực thi (Flow).
        /// </summary>
        private async Task<List<long>> CreateOrdersFromCartAsync(UserContext context, List<CartDetailDTO> items, string accountInfo, long txId)
        {
            var orderIds = new List<long>();
            
            foreach (var item in items)
            {
                var order = new Order
                {
                    UserId = context.UserId,
                    GamePackageId = item.ProductId,
                    UnitPrice = item.Price,
                    Quantity = item.Quantity,
                    GameAccountInfo = accountInfo,
                    Status = OrderStatus.Pending
                };
                
                var newOrderId = await _orderService.CreateOrderAsync(order, txId);
                orderIds.Add(newOrderId);
            }
            
            return orderIds;
        }
    }
}
