using TradeHub.BLL.Common;
using TradeHub.BLL.DTOs.Carts;
using TradeHub.BLL.Exceptions;
using TradeHub.DAL.Entities;
using TradeHub.DAL.Queries;
using TradeHub.DAL.Repositories;

namespace TradeHub.BLL.Services
{
    public class CartService : BaseService
    {
        private readonly CartItemRepository _cartItemRepo;
        private readonly CartItemQuery _cartItemQuery;

        public CartService(CartItemRepository cartItemRepo, CartItemQuery cartItemQuery, IIdentityService identityService)
            : base(identityService)
        {
            _cartItemRepo = cartItemRepo;
            _cartItemQuery = cartItemQuery;
        }

        // ===== READ (Giữ 'My' để khẳng định đây là giỏ hàng riêng tư) =====

        public async Task<List<CartItem>> GetMyItemsAsync() // Bỏ Cart vì đang ở CartService
        {
            return await _cartItemRepo.GetByUserIdAsync(CurrentUserId);
        }

        public async Task<List<CartDetailDTO>> GetMyDetailsAsync()
        {
            return await _cartItemQuery.GetCartDetailDTOsAsync(CurrentUserId);
        }

        public async Task<int> GetMyTotalQuantityAsync()
        {
            return await _cartItemRepo.GetTotalQuantityAsync(CurrentUserId);
        }

        // ===== ACTIONS (Thao tác nghiệp vụ - Bỏ 'My') =====

        public async Task<CartItem> AddItemAsync(CartItemRequest request)
        {
            if (request.Quantity <= 0)
                throw new BusinessException("Số lượng sản phẩm phải lớn hơn 0.");

            var cartItem = await _cartItemRepo.GetByUserProductAsync(CurrentUserId, request.ProductId);

            if (cartItem != null)
            {
                cartItem.Quantity += request.Quantity;
                await _cartItemRepo.UpdateQuantityAsync(CurrentUserId, cartItem.ProductId, cartItem.Quantity);
                return cartItem;
            }

            cartItem = new CartItem
            {
                UserId = CurrentUserId,
                ProductId = request.ProductId,
                Quantity = request.Quantity,
            };

            cartItem.Id = await _cartItemRepo.CreateAsync(cartItem);
            return cartItem;
        }

        public async Task UpdateItemQuantityAsync(int productId, int quantity)
        {
            if (quantity <= 0)
                throw new BusinessException("Số lượng sản phẩm phải lớn hơn 0.");

            var affected = await _cartItemRepo.UpdateQuantityAsync(CurrentUserId, productId, quantity);

            if (affected == 0)
                throw new BusinessException("Sản phẩm không tồn tại trong giỏ hàng của bạn.");
        }

        public async Task RemoveItemAsync(int productId)
        {
            var affected = await _cartItemRepo.DeleteAsync(CurrentUserId, productId);

            if (affected == 0)
                throw new BusinessException("Sản phẩm không tồn tại trong giỏ hàng.");
        }

        public async Task<int> ClearAsync()
        {
            // Xóa toàn bộ giỏ hàng của chính tôi
            return await _cartItemRepo.DeleteByUserIdAsync(CurrentUserId);
        }
    }
}