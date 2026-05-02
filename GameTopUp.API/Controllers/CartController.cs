using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GameTopUp.BLL.DTOs.Carts;
using GameTopUp.BLL.Services;

namespace GameTopUp.API.Controllers
{
    [Authorize]
    [Route("api/cart")]
    [ApiController]
    public class CartController : ApiControllerBase
    {
        private readonly CartService _cart;

        public CartController(CartService cart)
        {
            _cart = cart;
        }

        [HttpGet("items")]
        public async Task<IActionResult> GetCartItems()
        {
            var items = await _cart.GetDetailsAsync(CurrentUser);
            return ApiOk(items);
        }

        [HttpGet("items/summary")]
        public async Task<IActionResult> GetSummaryAsync()
        {
            var count = await _cart.GetTotalQuantityAsync(CurrentUser);
            return ApiOk(count);
        }

        [HttpPost("items")]
        public async Task<IActionResult> AddItem(CartItemRequest request)
        {
            var cartItem = await _cart.AddItemAsync(CurrentUser, request);
            return ApiCreated(cartItem, "Sản phẩm đã được thêm vào giỏ hàng.");
        }

        [HttpPut("items/{productId}")]
        public async Task<IActionResult> UpdateQuantity(long productId, [FromBody] int quantity)
        {
            await _cart.UpdateItemQuantityAsync(CurrentUser, productId, quantity);
            return ApiNoContent();
        }

        [HttpDelete("items/{productId}")]
        public async Task<IActionResult> RemoveCartItem(long productId)
        {
            await _cart.RemoveItemAsync(CurrentUser, productId);
            return ApiNoContent();
        }
    }
}
