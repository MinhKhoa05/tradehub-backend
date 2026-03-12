using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TradeHub.BLL.ApplicationServices;
using TradeHub.BLL.DTOs.Carts;
using TradeHub.BLL.Services;

namespace TradeHub.API.Controllers
{
    [Authorize]
    [Route("api/cart")]
    [ApiController]
    public class CartController : BaseController
    {
        private readonly CartService _cart;
        private readonly CartViewUsecase _cartView;

        public CartController(CartService cart, CartViewUsecase cartView)
        {
            _cart = cart;
            _cartView = cartView;
        }

        [HttpGet("items")]
        public async Task<IActionResult> GetCartItems()
        {
            var items = await _cartView.GetCartViewByUserIdAsync(CurrentUserId);
            return ApiOk(items);
        }

        [HttpGet("items/summary")]
        public async Task<IActionResult> GetSummaryAsync()
        {
            var count  = await _cart.GetTotalQuantityAsync(CurrentUserId);
            return ApiOk(count);
        }

        [HttpPost("items")]
        public async Task<IActionResult> AddItem(CartItemRequest request)
        {
            var cartItem = await _cart.AddItemAsync(CurrentUserId, request);
            return ApiCreated(cartItem);
        }

        [HttpPut("items/{productId}")]
        public async Task<IActionResult> UpdateQuantity(int productId, [FromBody] int quantity)
        {
            await _cart.UpdateQuantityAsync(CurrentUserId, productId, quantity);
            return ApiNoContent();
        }

        [HttpDelete("items/{productId}")]
        public async Task<IActionResult> RemoveCartItem(int productId)
        {
            await _cart.RemoveItemAsync(CurrentUserId, productId);
            return ApiNoContent();
        }
    }
}
