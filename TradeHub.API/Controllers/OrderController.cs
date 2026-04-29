using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TradeHub.BLL.ApplicationServices;
using TradeHub.BLL.Services;
using TradeHub.DAL.Entities;
using TradeHub.BLL.DTOs.Orders;

namespace TradeHub.API.Controllers
{
    [Authorize]
    [Route("api/orders")]
    [ApiController]
    public class OrderController : ApiControllerBase
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
            // Truyền UserContext để đảm bảo tính minh bạch và khả năng tái sử dụng logic
            var result = await _orderUseCase.CheckoutAsync(CurrentUser, request.GameAccountInfo);
            return ApiOk(result, "Đặt hàng thành công!");
        }

        [HttpGet("me")]
        public async Task<IActionResult> GetMyOrders([FromQuery] OrderStatus? status = null)
        {
            var orders = await _orderService.GetOrdersAsync(CurrentUser, status);
            return ApiOk(orders);
        }

        [HttpGet("{orderId}/history")]
        public async Task<IActionResult> GetOrderHistories(long orderId)
        {
            var histories = await _orderService.GetHistoriesAsync(orderId);
            return ApiOk(histories);
        }

        [HttpGet("{orderId}")]
        public async Task<IActionResult> GetOrderById(long orderId)
        {
            var order = await _orderService.GetByIdAsync(orderId);
            return ApiOk(order);
        }
    }
}
