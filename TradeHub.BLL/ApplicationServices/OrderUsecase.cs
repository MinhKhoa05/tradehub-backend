using TradeHub.BLL.DTOs.Carts;
using TradeHub.BLL.DTOs.Orders;
using TradeHub.BLL.Exceptions;
using TradeHub.BLL.Services;
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
        
        public OrderUsecase(ProductService product, OrderService order, CartService cart, WalletService wallet, DatabaseContext database)
        {
            _product = product;
            _order = order;
            _cart = cart;
            _wallet = wallet;
            _database = database;
        }

        public async Task PlaceOrderAsync(int userId, PaymentMethod paymentMethod)
        {
            var cartItemDTOs = await _cart.GetCartDetailDTOsAsync(userId);

            var productStockUpdates = cartItemDTOs
                .Select(item => new ProductStockUpdate(item.ProductId, item.Quantity))
                .ToList();

            // Map thành request order
            var request = new CheckoutRequest
            {
                PaymentMethod = paymentMethod,
                Items = MapToCheckoutItems(cartItemDTOs)
            };

            await _database.ExecuteInTransactionAsync(async () =>
            {
                // Cập nhật tồn kho sản phẩm
                await _product.DecreaseStockRangeAsync(productStockUpdates);

                // Tạo order
                var orderIds = await _order.CreateOrdersAsync(userId, request);

                if (paymentMethod == PaymentMethod.Wallet)
                {
                    var totalAmount = request.Items.Sum(i => i.UnitPrice * i.Quantity);
                    await _wallet.PayForOrdersAsync(userId, orderIds, totalAmount);
                }
            });

            await _cart.ClearCartAsync(userId);
        }

        private static List<CheckoutItem> MapToCheckoutItems(List<CartDetailDTO> cartItemDTOs)
        {
            return cartItemDTOs
                .Select(item => new CheckoutItem
                {
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    SellerId = item.SellerId,
                    UnitPrice = item.Price
                })
                .ToList();
        }
    }
}