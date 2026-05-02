using GameTopUp.BLL.Common;
using GameTopUp.BLL.DTOs.Carts;
using GameTopUp.BLL.Exceptions;
using GameTopUp.DAL.Entities;
using GameTopUp.DAL.Queries;
using GameTopUp.DAL.DTOs;
using GameTopUp.DAL.Repositories;

namespace GameTopUp.BLL.Services
{
    public class CartService
    {
        private readonly CartItemRepository _cartItemRepo;
        private readonly CartItemQuery _cartItemQuery;

        public CartService(CartItemRepository cartItemRepo, CartItemQuery cartItemQuery)
        {
            _cartItemRepo = cartItemRepo;
            _cartItemQuery = cartItemQuery;
        }

        public async Task<List<CartDetailDTO>> GetDetailsAsync(UserContext context)
        {
            return await _cartItemQuery.GetCartDetailDTOsAsync(context.UserId);
        }

        public async Task<int> GetTotalQuantityAsync(UserContext context)
        {
            return await _cartItemRepo.GetTotalQuantityAsync(context.UserId);
        }

        public async Task<CartItem> AddItemAsync(UserContext context, CartItemRequest request)
        {
            if (request.Quantity <= 0)
            {
                throw new BusinessException("Số lượng gói phải lớn hơn 0.");
            }

            var cartItem = await _cartItemRepo.GetByUserPackageAsync(context.UserId, request.ProductId);
            
            if (cartItem != null)
            {
                // Nếu sản phẩm đã tồn tại, ta chỉ cần cập nhật tăng số lượng
                cartItem.Quantity += request.Quantity;
                await _cartItemRepo.UpdateQuantityAsync(context.UserId, cartItem.GamePackageId, cartItem.Quantity);
                return cartItem;
            }

            // Tạo mới item trong giỏ hàng nếu chưa có
            cartItem = new CartItem 
            { 
                UserId = context.UserId, 
                GamePackageId = request.ProductId, 
                Quantity = request.Quantity 
            };
            
            cartItem.Id = await _cartItemRepo.CreateAsync(cartItem);
            return cartItem;
        }

        public async Task UpdateItemQuantityAsync(UserContext context, long productId, int quantity)
        {
            if (quantity <= 0)
            {
                throw new BusinessException("Số lượng sản phẩm phải lớn hơn 0.");
            }

            var rowsAffected = await _cartItemRepo.UpdateQuantityAsync(context.UserId, productId, quantity);
            if (rowsAffected == 0)
            {
                throw new BusinessException("Sản phẩm không tồn tại trong giỏ hàng.");
            }
        }

        public async Task RemoveItemAsync(UserContext context, long productId)
        {
            var rowsAffected = await _cartItemRepo.DeleteAsync(context.UserId, productId);
            if (rowsAffected == 0)
            {
                throw new BusinessException("Sản phẩm không tồn tại trong giỏ hàng.");
            }
        }

        public async Task<int> ClearAsync(UserContext context)
        {
            return await _cartItemRepo.DeleteByUserIdAsync(context.UserId);
        }
    }
}
