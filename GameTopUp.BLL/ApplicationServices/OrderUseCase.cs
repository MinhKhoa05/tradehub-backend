using GameTopUp.BLL.Common;
using GameTopUp.BLL.Services;
using GameTopUp.BLL.Exceptions;
using GameTopUp.DAL;
using GameTopUp.DAL.Entities;
using GameTopUp.DAL.Interfaces;
using GameTopUp.DAL.DTOs;
using GameTopUp.BLL.DTOs.Orders;
using GameTopUp.BLL.DTOs.Carts;

namespace GameTopUp.BLL.ApplicationServices
{
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

        public async Task<CheckoutResponseDTO> CheckoutAsync(UserContext context, string gameAccountInfo)
        {
            var cartItems = await _cartService.GetDetailsAsync(context);
            if (cartItems == null || !cartItems.Any())
            {
                throw new BusinessException("Giỏ hàng của bạn đang trống.");
            }

            decimal totalAmount = cartItems.Sum(item => item.Price * item.Quantity);

            return await _database.ExecuteInTransactionAsync(async () =>
            {
                var txResult = await _walletService.DeductMoneyAsync(context, totalAmount, "Thanh toán đơn hàng từ giỏ hàng.");
                
                var orderIds = await CreateOrdersFromCartAsync(cartItems, context, gameAccountInfo, txResult.TransactionId);
                
                await _cartService.ClearAsync(context);

                return new CheckoutResponseDTO { OrderIds = orderIds, TransactionId = txResult.TransactionId };
            });
        }

        private async Task<List<long>> CreateOrdersFromCartAsync(IEnumerable<CartDetailDTO> cartItems, UserContext context, string gameAccountInfo, long transactionId)
        {
            var orderIds = new List<long>();
            foreach (var item in cartItems)
            {
                var order = new Order
                {
                    UserId = context.UserId,
                    GamePackageId = item.ProductId,
                    UnitPrice = item.Price,
                    Quantity = item.Quantity,
                    GameAccountInfo = gameAccountInfo,
                    Status = OrderStatus.Pending,
                    WalletTransactionId = transactionId,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                
                var newOrderId = await _orderService.CreateOrder(order, context);
                orderIds.Add(newOrderId);
            }
            return orderIds;
        }

        public async Task PickOrderAsync(long orderId, UserContext adminContext)
        {
            await _database.ExecuteInTransactionAsync(async () =>
            {
                var order = await _orderService.LockAndGetByIdAsync(orderId);
                await _orderService.PickOrder(order, adminContext);
            });
        }

        public async Task CompleteOrderAsync(long orderId, UserContext adminContext)
        {
            await _database.ExecuteInTransactionAsync(async () =>
            {
                var order = await _orderService.LockAndGetByIdAsync(orderId);
                await _orderService.CompleteOrder(order, adminContext);
            });
        }

        public async Task CancelOrderAsync(long orderId, UserContext adminContext, string? reason = null)
        {
            await _database.ExecuteInTransactionAsync(async () =>
            {
                var order = await _orderService.LockAndGetByIdAsync(orderId);

                var isNewCancellation = await _orderService.CancelOrder(order, adminContext, reason);

                if (isNewCancellation)
                {
                    var userContext = new UserContext { UserId = order.UserId };
                    await _walletService.RefundMoneyAsync(userContext, order.Total, $"Hoàn tiền đơn hàng #{orderId}. {reason ?? ""}");
                }
            });
        }
    }
}
