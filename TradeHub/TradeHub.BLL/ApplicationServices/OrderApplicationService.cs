using TradeHub.BLL.DTOs.Carts;
using TradeHub.BLL.DTOs.Orders;
using TradeHub.BLL.Services;
using TradeHub.DAL;
using TradeHub.DAL.Entities;

namespace TradeHub.BLL.ApplicationServices
{
    public class OrderApplicationService
    {
        private readonly ProductService _productService;
        private readonly OrderService _orderService;
        private readonly CartService _cartService;
        private readonly DatabaseContext _databaseContext;

        public OrderApplicationService(
            ProductService productService,
            OrderService orderService,
            CartService cartService,
            DatabaseContext databaseContext)
        {
            _productService = productService;
            _orderService = orderService;
            _cartService = cartService;
            _databaseContext = databaseContext;
        }

        public async Task<List<int>> PlaceOrderAsync(int userId, PaymentMethod paymentMethod)
        {
            return await _databaseContext.ExecuteInTransactionAsync(async () =>
            {
                var cartItems = await GetCartItemsAsync(userId);

                var products = await GetProductsAsync(cartItems);

                await ValidateAndUpdateStockAsync(cartItems, products);

                var checkoutItems = MapToCheckoutItems(cartItems, products);

                var request = new CheckoutRequest
                {
                    PaymentMethod = paymentMethod,
                    Items = checkoutItems
                };

                var orderIds = await _orderService.CreateOrdersAsync(userId, request);

                await _cartService.ClearCartAsync(userId);

                return orderIds;
            });
        }

        private static List<CheckoutItem> MapToCheckoutItems(List<CartItem> cartItems, List<Product> products)
        {
            var productDict = products.ToDictionary(p => p.Id);

            return cartItems.Select(cart =>
            {
                var product = productDict[cart.ProductId];

                return new CheckoutItem
                {
                    ProductId = product.Id,
                    SellerId = product.SellerId,
                    Quantity = cart.Quantity,
                    UnitPrice = product.Price
                };
            }).ToList();
        }

        private async Task<List<CartItem>> GetCartItemsAsync(int userId)
        {
            var cartItems = await _cartService.GetCartByUserIdAsync(userId);

            if (cartItems.Count == 0)
                throw new InvalidOperationException("Cart is empty");

            return cartItems;
        }

        private async Task<List<Product>> GetProductsAsync(List<CartItem> cartItems)
        {
            var productIds = cartItems.Select(x => x.ProductId);
            return await _productService.GetProductsByIdsAsync(productIds);
        }

        private async Task ValidateAndUpdateStockAsync(List<CartItem> cartItems, List<Product> products)
        {
            foreach (var cart in cartItems)
            {
                var product = products.First(p => p.Id == cart.ProductId);

                // Kiểm tra tầng 1
                if (product.Stock < cart.Quantity)
                    throw new InvalidOperationException($"Product {product.Id} out of stock");
                
                // Cập nhật số lượng (kiểm tra tầng 2), service sẽ tự quăng lỗi nếu có
                await _productService.DecreaseStockForOrderAsync(product.Id, cart.Quantity);
            }
        }
    }
}