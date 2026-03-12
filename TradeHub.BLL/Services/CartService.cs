using TradeHub.BLL.DTOs.Carts;
using TradeHub.BLL.Exceptions;
using TradeHub.DAL.Entities;
using TradeHub.DAL.Repositories;

namespace TradeHub.BLL.Services
{
    public class CartService
    {
        private readonly CartItemRepository _cartItemRepo;

        public CartService(CartItemRepository cartItemRepo)
        {
            _cartItemRepo = cartItemRepo;
        }

        public async Task<List<CartItem>> GetCartItemsByUserIdAsync(int userId)
        {
            return await _cartItemRepo.GetByUserIdAsync(userId);
        }

        public async Task<int> GetTotalQuantityAsync(int userId)
        {
            return await _cartItemRepo.GetTotalQuantityAsync(userId);
        }
        
        public async Task<CartItem> AddItemAsync(int userId, CartItemRequest request)
        {
            if (request.Quantity <= 0)
                throw new BusinessException("Số lượng phải lớn hơn 0");

            var cartItem = await _cartItemRepo.GetByUserProductAsync(userId, request.ProductId);

            if (cartItem != null)
            {
                cartItem.Quantity += request.Quantity;
                await _cartItemRepo.UpdateQuantityAsync(userId, cartItem.ProductId, cartItem.Quantity);
                return cartItem;
            }

            cartItem = new CartItem
            {
                UserId = userId,
                ProductId = request.ProductId,
                Quantity = request.Quantity,
            };

            cartItem.Id = await _cartItemRepo.CreateAsync(cartItem);

            return cartItem;
        }

        public async Task UpdateQuantityAsync(int userId, int productId, int quantity)
        {
            if (quantity <= 0)
                throw new BusinessException("Số lượng phải lớn hơn 0");

            var affected = await _cartItemRepo.UpdateQuantityAsync(userId, productId, quantity);
            if (affected == 0)
                throw new BusinessException("Sản phẩm không tồn tại trong giỏ hàng");
        }

        public async Task RemoveItemAsync(int userId, int productId)
        {
            var affected = await _cartItemRepo.DeleteAsync(userId, productId);

            if (affected == 0)
                throw new BusinessException("Vật phẩm không tồn tại trong giỏ hàng");
        }

        public async Task ClearAsync(int userId)
        {
            await _cartItemRepo.DeleteByUserIdAsync(userId);
        }
    }
}
