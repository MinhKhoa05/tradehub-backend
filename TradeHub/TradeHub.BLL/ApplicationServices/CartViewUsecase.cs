using TradeHub.BLL.Services;
using TradeHub.BLL.DTOs.Carts;
using TradeHub.BLL.Mappings;

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
                .Distinct()
                .ToList();

            var products = await _productService.GetProductsByIdsAsync(productIds);

            var productDic = products.ToDictionary(p => p.Id);

            return cartItems
                .Select(cartItem =>
                    cartItem.ToCartItemDTO(
                        productDic.GetValueOrDefault(cartItem.ProductId)))
                .ToList();
        }
    }
}