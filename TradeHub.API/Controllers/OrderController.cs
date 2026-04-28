using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TradeHub.BLL.ApplicationServices;
using TradeHub.BLL.Services;
using TradeHub.DAL.Entities;
using TradeHub.BLL.DTOs.Orders;
using System.Security.Claims;

namespace TradeHub.API.Controllers
{
    [Authorize]
    [Route("api/orders")]
    [ApiController]
    public class OrderController : BaseController
    {
        private readonly OrderUseCase _orderUseCase;
        private readonly OrderService _orderService;

        public OrderController(OrderUseCase orderUseCase, OrderService orderService)
        {
            _orderUseCase = orderUseCase;
            _orderService = orderService;
        }

        [HttpPost("checkout")]
        public async Task<IActionResult> Checkout([FromBody] CheckoutRequest request)
        {
            // Lấy UserId từ Token
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!long.TryParse(userIdStr, out long userId))
            {
                return Unauthorized();
            }

            var result = await _orderUseCase.CheckoutAsync(userId, request.CartId, request.GameAccountInfo);
            
            if (!result.IsSuccess)
            {
                return BadRequest(result);
            }

            return ApiOk(result.Data, result.Message);
        }

        [HttpGet("me")]
        public async Task<IActionResult> GetMyOrders([FromQuery] OrderStatus? status = null)
        {
            var orders = await _orderService.GetMyOrdersAsync(status);
            return ApiOk(orders);
        }

        [HttpGet("{orderId}/history")]
        public async Task<IActionResult> GetOrderHistories(long orderId)
        {
            var histories = await _orderService.GetHistoriesAsync(orderId);
            return ApiOk(histories);
        }

        [HttpGet("{orderId}/items")]
        public async Task<IActionResult> GetOrderItems(long orderId)
        {
            var order = await _orderService.GetItemsAsync(orderId);
            return ApiOk(order);
        }
    }
}
