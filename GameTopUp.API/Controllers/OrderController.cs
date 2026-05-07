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

        [HttpPost("place")]
        public async Task<IActionResult> PlaceOrder([FromBody] PlaceOrderRequestDTO request)
        {
            var result = await _orderUseCase.PlaceOrderAsync(CurrentUser, request);
            return ApiCreated(result, "Đặt hàng thành công!");
        }

        [HttpPost("{orderId}/pay")]
        public async Task<IActionResult> PayOrder(long orderId)
        {
            await _orderUseCase.PayOrderAsync(orderId, CurrentUser);
            return ApiOk(null, "Thanh toán đơn hàng thành công");
        }

        [HttpGet("me")]
        public async Task<IActionResult> GetMyOrders([FromQuery] OrderStatus? status = null)
        {
            var orders = await _orderService.GetOrdersByUserAsync(CurrentUser, status);
            return ApiOk(orders);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> GetOrders([FromQuery] OrderStatus? status = null)
        {
            var orders = await _orderService.GetAllOrdersAsync(status);
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
            await _orderUseCase.PickOrderAsync(orderId, CurrentUser);
            return ApiOk(null, "Bạn đã tiếp nhận đơn hàng thành công.");
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("{orderId}/complete")]
        public async Task<IActionResult> CompleteOrder(long orderId)
        {
            await _orderUseCase.CompleteOrderAsync(orderId, CurrentUser);
            return ApiOk(null, "Đơn hàng đã được hoàn thành thành công.");
        }

        [HttpPost("{orderId}/cancel")]
        public async Task<IActionResult> CancelOrder(long orderId)
        {
            await _orderUseCase.CancelOrderAsync(orderId, CurrentUser);
            return ApiOk(null, "Đơn hàng đã được hủy và hoàn tiền thành công.");
        }
    }
}
