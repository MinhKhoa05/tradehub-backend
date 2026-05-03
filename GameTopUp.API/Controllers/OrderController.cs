using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GameTopUp.BLL.ApplicationServices;
using GameTopUp.BLL.Services;
using GameTopUp.DAL.Entities;
using GameTopUp.BLL.DTOs.Orders;

namespace GameTopUp.API.Controllers
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

        [Authorize(Roles = "Admin")]
        [HttpPost("{orderId}/pick")]
        public async Task<IActionResult> PickOrder(long orderId)
        {
            await _orderService.PickOrderAsync(orderId, CurrentUser);
            return ApiOk(null, "Bạn đã tiếp nhận đơn hàng thành công.");
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("{orderId}/complete")]
        public async Task<IActionResult> CompleteOrder(long orderId)
        {
            await _orderService.CompleteOrderAsync(orderId, CurrentUser);
            return ApiOk(null, "Đơn hàng đã được hoàn thành thành công.");
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("{orderId}/cancel")]
        public async Task<IActionResult> CancelOrder(long orderId)
        {
            await _orderUseCase.CancelOrderAsync(orderId, CurrentUser);
            return ApiOk(null, "Đơn hàng đã được hủy và hoàn tiền thành công.");
        }
    }
}
