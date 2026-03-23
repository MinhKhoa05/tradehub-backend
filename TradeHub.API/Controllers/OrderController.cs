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
    public class OrderController : BaseController
    {
        private readonly OrderUsecase _orderUsecase;
        private readonly OrderService _orderService;

        public OrderController(OrderUsecase orderUsecase, OrderService orderService)
        {
            _orderUsecase = orderUsecase;
            _orderService = orderService;
        }

        [HttpPost]
        public async Task<IActionResult> PlaceOrder()
        {
            await _orderUsecase.PlaceOrderAsync(CurrentUserId, PaymentMethod.Cod);
            return ApiOk();
        }

        [HttpGet("me")]
        public async Task<IActionResult> GetMyOrders([FromQuery] OrderType type = OrderType.All)
        {
            var orders = await _orderService.GetOrdersByIdAsync(CurrentUserId, type);
            return ApiOk(orders);
        }

        [HttpGet("{orderId}/history")]
        public async Task<IActionResult> GetOrderHistories(int orderId)
        {
            var histories = await _orderService.GetOrderHistoriesAsync(CurrentUserId, orderId);
            return ApiOk(histories);
        }

        [HttpGet("{orderId}/items")]
        public async Task<IActionResult> GetOrderItems(int orderId)
        {
            var histories = await _orderService.GetOrderItemsAsync(CurrentUserId, orderId);
            return ApiOk(histories);
        }
    }
}
