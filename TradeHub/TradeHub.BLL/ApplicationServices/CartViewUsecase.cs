using TradeHub.BLL.Services;
using TradeHub.BLL.DTOs.Carts;
using TradeHub.BLL.Mappings;

namespace TradeHub.BLL.ApplicationServices
{
    public class CartViewUsecase
    {
        private readonly CartService _cart;
        private readonly ProductService _product;

        public CartViewUsecase(CartService cart, ProductService product)
        {
            _cart = cart;
            _product = product;
        }

        public async Task<IEnumerable<CartItemDTO>> GetCartViewByUserIdAsync(int userId)
        {
            var cartItems = await _cart.GetCartItemsByUserIdAsync(userId);
            
            var productIds = cartItems.Select(x => x.ProductId).ToList();

            var products = await _product.GetProductsByIdsAsync(productIds);

            var productDict = products.ToDictionary(p => p.Id);

            return cartItems.Select(cartItem =>
                cartItem.ToCartItemDTO(
                    productDict.GetValueOrDefault(cartItem.ProductId)
                )
            );
        }
    }
}