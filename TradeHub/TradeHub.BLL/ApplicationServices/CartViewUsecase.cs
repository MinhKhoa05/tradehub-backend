using TradeHub.BLL.Services;
using TradeHub.BLL.DTOs.Carts;

namespace TradeHub.BLL.ApplicationServices
{
    public class CartViewUsecase
    {
        private readonly CartService _cartService;
        private readonly ProductService _productService;

        public CartViewUsecase(CartService cartService, ProductService productService)
        {
            _cartService = cartService;
            _productService = productService;
        }

        public async Task<List<CartItemDTO>> GetCartViewByUserIdAsync(int userId)
        {
            var cartItems = await _cartService.GetCartByUserIdAsync(userId);

            if (!cartItems.Any())
                return new List<CartItemDTO>();

            var productIds = cartItems
                .Select(x => x.ProductId)
                .Distinct();

            var products = await _productService.GetProductsByIdsAsync(productIds);

            var productDic = products.ToDictionary(p => p.Id);

            return cartItems.Select(cartItem =>
            {
                productDic.TryGetValue(cartItem.ProductId, out var product);

                return new CartItemDTO
                {
                    Id = cartItem.Id,
                    ProductId = cartItem.ProductId,
                    Quantity = cartItem.Quantity,

                    Product = product == null ? null : new CartProductDTO
                    {
                        ProductId = product.Id,
                        Name = product.Name,
                        Price = product.Price
                    }
                };
            }).ToList();
        }
    }
}
