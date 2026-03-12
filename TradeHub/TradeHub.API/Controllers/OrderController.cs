using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TradeHub.BLL.ApplicationServices;
using TradeHub.DAL.Entities;

namespace TradeHub.API.Controllers
{
    [Authorize]
    [Route("api/orders")]
    [ApiController]
    public class OrderController : BaseController
    {
        private readonly OrderUsecase _orderUsecase;

        public OrderController(OrderUsecase orderUsecase)
        {
            _orderUsecase = orderUsecase;
        }

        [HttpPost]
        public async Task<IActionResult> PlaceOrder()
        {
            var orderIds = await _orderUsecase.PlaceOrderAsync(CurrentUserId, PaymentMethod.Cod);
            return ApiOk(orderIds);
        }
    }
}
