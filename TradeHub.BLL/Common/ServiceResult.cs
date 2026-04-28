namespace TradeHub.BLL.Common
{
    /// <summary>
    /// ServiceResult chuẩn hóa phản hồi từ tầng logic (BLL) lên tầng hiển thị (API).
    /// Việc sử dụng một cấu trúc phản hồi thống nhất giúp frontend dễ dàng xử lý các trạng thái
    /// thành công hoặc thất bại mà không cần quan tâm đến chi tiết kỹ thuật bên trong.
    /// </summary>
    public class ServiceResult
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; } = string.Empty;
        public object? Data { get; set; }
        public ServiceErrorCode ErrorCode { get; set; } = ServiceErrorCode.None;

        public static ServiceResult Success(string message = "Thao tác thành công", object? data = null)
        {
            return new ServiceResult 
            { 
                IsSuccess = true, 
                Message = message, 
                Data = data 
            };
        }

        public static ServiceResult Failure(string message, ServiceErrorCode errorCode = ServiceErrorCode.BadRequest)
        {
            return new ServiceResult 
            { 
                IsSuccess = false, 
                Message = message,
                ErrorCode = errorCode
            };
        }
    }

    public enum ServiceErrorCode
    {
        None = 0,
        BadRequest = 400,
        Unauthorized = 401,
        Forbidden = 403,
        NotFound = 404,
        InternalError = 500
    }

    /// <summary>
    /// Phiên bản Generic của ServiceResult giúp tầng Service trả về dữ liệu có kiểu tường minh (Strongly Typed).
    /// </summary>
    public class ServiceResult<T> : ServiceResult
    {
        public new T? Data { get; set; }

        public static ServiceResult<T> Success(T data, string message = "Thao tác thành công")
        {
            return new ServiceResult<T> 
            { 
                IsSuccess = true, 
                Message = message, 
                Data = data 
            };
        }

        public new static ServiceResult<T> Failure(string message, ServiceErrorCode errorCode = ServiceErrorCode.BadRequest)
        {
            return new ServiceResult<T> 
            { 
                IsSuccess = false, 
                Message = message,
                ErrorCode = errorCode
            };
        }
    }
}
