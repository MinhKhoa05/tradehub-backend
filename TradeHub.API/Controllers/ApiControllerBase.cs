using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TradeHub.BLL.Common;

namespace TradeHub.API.Controllers
{
    /// <summary>
    /// Base Controller cung cấp các phương thức tiện ích và đóng gói thông tin người dùng.
    /// Việc tập trung logic trích xuất UserContext tại đây giúp các Controller con sạch sẽ hơn
    /// và đảm bảo tính nhất quán khi truyền dữ liệu định danh xuống tầng UseCase/Service.
    /// </summary>
    [ApiController]
    public abstract class ApiControllerBase : ControllerBase
    {
        /// <summary>
        /// Trích xuất thông tin từ JWT Token và đóng gói vào UserContext.
        /// Việc khởi tạo UserContext ngay tại Controller giúp tầng Service không bị phụ thuộc vào HttpContext,
        /// từ đó hỗ trợ tốt cho việc viết Unit Test và chạy Background Jobs.
        /// </summary>
        protected UserContext CurrentUser
        {
            get
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null)
                {
                    return null!;
                }

                return new UserContext
                {
                    UserId = long.Parse(userIdClaim.Value),
                    Username = User.FindFirst(ClaimTypes.Name)?.Value ?? string.Empty,
                    Role = User.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty
                };
            }
        }

        protected IActionResult ApiOk(object? data = null, string? message = null)
        {
            return Ok(ApiResponse.Ok(data, message));
        }

        protected IActionResult ApiCreated(object? data = null, string? message = null)
        {
            return StatusCode(201, ApiResponse.Ok(data, message));
        }

        protected IActionResult ApiResult(ServiceResult result)
        {
            if (result.IsSuccess)
            {
                return Ok(ApiResponse.Ok(result.Data, result.Message));
            }

            var statusCode = result.ErrorCode switch
            {
                ServiceErrorCode.NotFound => 404,
                ServiceErrorCode.Unauthorized => 401,
                ServiceErrorCode.Forbidden => 403,
                ServiceErrorCode.InternalError => 500,
                _ => 400
            };

            return StatusCode(statusCode, ApiResponse.Fail(result.Message));
        }

        protected IActionResult ApiNoContent()
        {
            return NoContent();
        }
    }
}
