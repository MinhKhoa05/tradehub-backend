using Microsoft.AspNetCore.Mvc;
using TradeHub.API.Extensions;

namespace TradeHub.API.Controllers
{
    public abstract class BaseController : ControllerBase
    {
        protected int CurrentUserId => HttpContext.GetUserId();
        
        protected IActionResult ApiOk(object? data = null, string? message = null)
        {
            return Ok(ApiResponse.Ok(data, message));
        }

        protected IActionResult ApiCreated(object? data = null, string? message = null)
        {
            return StatusCode(201, ApiResponse.Ok(data, message));
        }

        protected IActionResult ApiNoContent()
        {
            return NoContent();
        }
    }
}
