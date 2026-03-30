using Microsoft.AspNetCore.Mvc;

namespace TradeHub.API.Controllers
{
    public abstract class BaseController : ControllerBase
    {
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
