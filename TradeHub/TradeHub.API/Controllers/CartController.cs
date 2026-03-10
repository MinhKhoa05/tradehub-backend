using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TradeHub.API.Extensions;
using TradeHub.BLL.ApplicationServices;
using TradeHub.BLL.DTOs.Carts;
using TradeHub.BLL.Services;

namespace TradeHub.API.Controllers
{
    [Route("api/cart")]
    [ApiController]
    public class CartController : ControllerBase
    {
        private readonly CartService _cartService;
        private readonly CartViewUsecase _cartViewUsecase;

        public CartController(CartService cartService, CartViewUsecase cartViewUsecase)
        {
            _cartService = cartService;
            _cartViewUsecase = cartViewUsecase;
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetMyCart()
        {
            var userId = HttpContext.GetUserId();

            var cartProducts = await _cartViewUsecase.GetCartViewByUserIdAsync(userId);
            return Ok(cartProducts);
        }

        [Authorize]
        [HttpGet("count")]
        public async Task<IActionResult> GetCartItemCount()
        {
            var userId = HttpContext.GetUserId();
            var count = await _cartService.GetCartItemCountAsync(userId);
            return Ok(count);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> AddToCart(CartItemRequest request)
        {
            var userId = HttpContext.GetUserId();

            var cartItem = await _cartService.AddToCartAsync(userId, request);
            return Ok(cartItem);
        }

        [Authorize]
        [HttpPut]
        public async Task<IActionResult> UpdateQuantity(CartItemRequest request)
        {
            var userId = HttpContext.GetUserId();

            await _cartService.UpdateQuantityAsync(userId, request);
            return Ok();
        }

        [Authorize]
        [HttpDelete("{productId}")]
        public async Task<IActionResult> RemoveCartItem(int productId)
        {
            var userId = HttpContext.GetUserId();

            await _cartService.RemoveCartItemAsync(userId, productId);
            return Ok();
        }
    }
}
