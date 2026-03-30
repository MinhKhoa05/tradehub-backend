using TradeHub.BLL.DTOs.Orders;
using TradeHub.BLL.Services;
using TradeHub.BLL.Exceptions;
using TradeHub.DAL;
using TradeHub.DAL.DTOs;
using TradeHub.DAL.Entities;
using TradeHub.DAL.Queries;

namespace TradeHub.BLL.ApplicationServices
{
    public class OrderUsecase
    {
        private readonly ProductService _product;
        private readonly OrderService _order;
        private readonly CartService _cart;
        private readonly WalletService _wallet;
        private readonly DatabaseContext _database;

        public OrderUsecase(
            ProductService product,
            OrderService order,
            CartService cart,
            WalletService wallet,
            DatabaseContext database)
        {
            _product = product;
            _order = order;
            _cart = cart;
            _wallet = wallet;
            _database = database;
        }

        // Đổi tên từ PlaceMyOrder sang PlaceOrderAsync cho chuyên nghiệp
        public async Task PlaceOrderAsync(PaymentMethod paymentMethod)
        {
            // 1. Lấy thông tin giỏ hàng (Read - Giữ My)
            var cartItems = await _cart.GetMyDetailsAsync();

            if (cartItems.Count == 0)
                throw new BusinessException("Giỏ hàng của bạn đang trống.");

            var totalAmount = cartItems.Sum(i => i.Price * i.Quantity);

            // 2. Kiểm tra số dư ví (Action - Bỏ My theo Service đã sửa)
            if (paymentMethod == PaymentMethod.Wallet)
            {
                await _wallet.EnsureBalanceIsEnoughAsync(totalAmount);
            }

            var stockUpdates = cartItems
                .Select(item => new ProductStockUpdate(item.ProductId, item.Quantity))
                .ToList();

            var request = new CheckoutRequest
            {
                PaymentMethod = paymentMethod,
                Items = MapToCheckoutItems(cartItems)
            };

            // 3. Thực thi nghiệp vụ phức tạp trong Transaction
            await _database.ExecuteInTransactionAsync(async () =>
            {
                // Trừ tồn kho (Hành động khách quan)
                await _product.DecreaseStockRangeAsync(stockUpdates);

                // Tạo đơn hàng (Bỏ My)
                var orderIds = await _order.CreateOrdersAsync(request);

                // Thanh toán (Bỏ My)
                if (paymentMethod == PaymentMethod.Wallet)
                {
                    await _wallet.PayForOrdersAsync(orderIds, totalAmount);
                }

                // Xóa giỏ hàng (Bỏ My)
                var affected = await _cart.ClearAsync();

                if (affected == 0)
                    throw new BusinessException("Không thể hoàn tất đặt hàng do giỏ hàng không tồn tại.");
            });
        }

        private static List<CheckoutItem> MapToCheckoutItems(List<CartDetailDTO> cartItems)
        {
            return cartItems.Select(item => new CheckoutItem
            {
                ProductId = item.ProductId,
                Quantity = item.Quantity,
                SellerId = item.SellerId,
                UnitPrice = item.Price
            }).ToList();
        }
    }
}