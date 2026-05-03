using System.Net;
using System.Text.Json;
using GameTopUp.BLL.Exceptions;

namespace GameTopUp.API.Middlewares
{
    /// <summary>
    /// Middleware xử lý ngoại lệ tập trung cho toàn bộ ứng dụng.
    /// Việc gom lỗi về một nơi giúp đảm bảo phản hồi trả về Client luôn thống nhất về cấu trúc,
    /// đồng thời bảo mật thông tin bằng cách không để lộ chi tiết lỗi kỹ thuật (Stack Trace).
    /// </summary>
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;

        public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                // Nếu Response đã bắt đầu gửi (Headers đã gửi), ta không được phép ghi đè StatusCode hay Content nữa.
                if (context.Response.HasStarted)
                {
                    _logger.LogWarning("Response đã bắt đầu gửi về client, không thể can thiệp thêm vào Middleware.");
                    return;
                }

                _logger.LogError(ex, ex.Message);
                await HandleExceptionAsync(context, ex);
            }
        }

        private static async Task HandleExceptionAsync(HttpContext context, Exception ex)
        {
            context.Response.ContentType = "application/json";

            // Phân loại mã lỗi HTTP dựa trên kiểu Exception ném ra từ tầng nghiệp vụ.
            var statusCode = HttpStatusCode.InternalServerError;
            
            if (ex is NotFoundException)
            {
                statusCode = HttpStatusCode.NotFound;
            }
            else if (ex is UnauthorizedException || ex is UnauthorizedAccessException)
            {
                statusCode = HttpStatusCode.Unauthorized;
            }
            else if (ex is ForbiddenException)
            {
                statusCode = HttpStatusCode.Forbidden;
            }
            else if (ex is BusinessException)
            {
                statusCode = HttpStatusCode.BadRequest;
            }

            context.Response.StatusCode = (int)statusCode;

            // Nếu là lỗi hệ thống (500), ẩn chi tiết lỗi để bảo mật. 
            // Ngược lại, trả về thông báo lỗi nghiệp vụ cụ thể cho người dùng.
            var message = "Hệ thống đang bận một chút hoặc có sự cố nhỏ. Bạn vui lòng thử lại sau vài giây nhé!";
            if (statusCode != HttpStatusCode.InternalServerError)
            {
                message = ex.Message;
            }

            var response = ApiResponse.Fail(message);

            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            await context.Response.WriteAsync(JsonSerializer.Serialize(response, options));
        }
    }
}
