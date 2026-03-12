using TradeHub.BLL.DTOs.Orders;
using TradeHub.BLL.Exceptions;
using TradeHub.BLL.Services;
using TradeHub.DAL;
using TradeHub.DAL.Entities;

namespace TradeHub.BLL.ApplicationServices
{
    public class OrderUsecase
    {
        private readonly ProductService _product;
        private readonly OrderService _order;
        private readonly CartService _cart;
        private readonly DatabaseContext _database;
        
        public OrderUsecase(ProductService product, OrderService order, CartService cart, DatabaseContext database)
        {
            _product = product;
            _order = order;
            _cart = cart;
            _database = database;
        }

        public async Task<List<int>> PlaceOrderAsync(int userId, PaymentMethod paymentMethod)
        {
            return await _database.ExecuteInTransactionAsync(async () =>
            {
                var cartItems = await GetCartItemsAsync(userId);

                var productDict = await GetProductDictAsync(cartItems);

                await ValidateAndUpdateStockAsync(cartItems, productDict);

                var request = new CheckoutRequest
                {
                    PaymentMethod = paymentMethod,
                    Items = MapToCheckoutItems(cartItems, productDict)
                };

                var orderIds = await _order.CreateOrdersAsync(userId, request);

                await _cart.ClearAsync(userId);

                return orderIds;
            });
        }

        private static List<CheckoutItem> MapToCheckoutItems(List<CartItem> cartItems, Dictionary<int, Product> productDict)
        {
            return cartItems.Select(cartItem =>
            {
                var product = productDict[cartItem.ProductId];

                return new CheckoutItem
                {
                    ProductId = product.Id,
                    SellerId = product.SellerId,
                    Quantity = cartItem.Quantity,
                    UnitPrice = product.Price
                };
            }).ToList();
        }

        private async Task<List<CartItem>> GetCartItemsAsync(int userId)
        {
            var cartItems = await _cart.GetCartItemsByUserIdAsync(userId);

            if (cartItems.Count == 0)
                throw new BusinessException("Cart is empty");

            return cartItems;
        }

        private async Task<Dictionary<int, Product>> GetProductDictAsync(List<CartItem> cartItems)
        {
            var productIds = cartItems.Select(x => x.ProductId).ToList();
            var products = await _product.GetProductsByIdsAsync(productIds);

            if (products.Count != productIds.Count)
                throw new BusinessException("Một vài sản phẩm không tồn tại");

            return products.ToDictionary(p => p.Id);
        }

        private async Task ValidateAndUpdateStockAsync(List<CartItem> cartItems, Dictionary<int, Product> productDict)
        {
            foreach (var cartItem in cartItems)
            {
                var product = productDict[cartItem.ProductId];

                // Kiểm tra tầng 1
                if (product.Stock < cartItem.Quantity)
                    throw new BusinessException($"Product {product.Id} out of stock");
                
                // Cập nhật số lượng (kiểm tra tầng 2), service sẽ tự quăng lỗi nếu có
                await _product.DecreaseStockForOrderAsync(product.Id, cartItem.Quantity);
            }
        }
    }
}