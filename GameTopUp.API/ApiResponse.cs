namespace GameTopUp.API
{
    /// <summary>
    /// Cấu trúc phản hồi chuẩn (Standard Response Body) cho toàn bộ API của hệ thống.
    /// Việc thống nhất cấu trúc này giúp Frontend dễ dàng xây dựng các Interceptor xử lý lỗi 
    /// và hiển thị thông báo một cách tự động.
    /// </summary>
    public class ApiResponse
    {
        public bool Success { get; set; }

        public string? Message { get; set; }

        public object? Data { get; set; }

        private ApiResponse() 
        { 
        }

        public static ApiResponse Ok(object? data = null, string? message = null)
        {
            return new ApiResponse
            {
                Success = true,
                Data = data,
                Message = message
            };
        }

        public static ApiResponse Fail(string message, object? data = null)
        {
            return new ApiResponse
            {
                Success = false,
                Message = message,
                Data = data
            };
        }
    }
}
