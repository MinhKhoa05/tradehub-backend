using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TradeHub.API.Extensions;
using TradeHub.BLL.ApplicationServices;
using TradeHub.DAL.Entities;

namespace TradeHub.API.Controllers
{
    [Route("api/orders")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly OrderApplicationService _orderApplicationService;

        public OrderController(OrderApplicationService orderApplicationService)
        {
            _orderApplicationService = orderApplicationService;
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> PlaceOrder()
        {
            var userId = HttpContext.GetUserId();
            var orderIds = await _orderApplicationService.PlaceOrderAsync(userId, PaymentMethod.Cod);
            return Ok(orderIds);
        }
    }
}
