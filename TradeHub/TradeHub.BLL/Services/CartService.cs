using TradeHub.BLL.DTOs.Carts;
using TradeHub.BLL.Exceptions;
using TradeHub.DAL.Entities;
using TradeHub.DAL.Repositories;

namespace TradeHub.BLL.Services
{
    public class CartService
    {
        private readonly CartItemRepository _cartItemRepository;

        public CartService(CartItemRepository cartItemRepository)
        {
            _cartItemRepository = cartItemRepository;
        }

        public async Task<List<CartItem>> GetCartByUserIdAsync(int userId)
        {
            return await _cartItemRepository.GetByUserIdAsync(userId);
        }

        public async Task<int> GetCartItemCountAsync(int userId)
        {
            return await _cartItemRepository.GetCartItemCountAsync(userId);
        }

        public async Task<CartItem> AddToCartAsync(int userId, CartItemRequest request)
        {
            var cartItem = await _cartItemRepository.GetByUserProductAsync(userId, request.ProductId);
            if (cartItem == null)
            {
                cartItem = new CartItem
                {
                    UserId = userId,
                    ProductId = request.ProductId,
                    Quantity = request.Quantity,
                };

                cartItem.Id = await _cartItemRepository.CreateAsync(cartItem);
            }
            else
            {
                cartItem.Quantity += request.Quantity;
                await _cartItemRepository.UpdateQuantityAsync(userId, cartItem.ProductId, cartItem.Quantity);
            }

            return cartItem;
        }

        public async Task UpdateQuantityAsync(int userId, CartItemRequest request)
        {
            int affected = await _cartItemRepository.UpdateQuantityAsync(userId, request.ProductId, request.Quantity);
            if (affected == 0)
            {
                throw new BusinessException();
            }
        }

        public async Task RemoveCartItemAsync(int userId, int productId)
        {
            int affected = await _cartItemRepository.DeleteAsync(userId, productId);
            if (affected == 0)
            {
                throw new BusinessException("Vật phẩm không tồn tại trong giỏ hàng");
            }
        }

        public async Task ClearCartAsync(int userId)
        {
            int affected = await _cartItemRepository.DeleteByUserIdAsync(userId);
            if (affected == 0)
            {
                throw new BusinessException("Giỏ hàng trống");
            }
        }
    }
}
