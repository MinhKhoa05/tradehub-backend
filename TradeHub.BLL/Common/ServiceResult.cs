namespace TradeHub.BLL.Common
{
    public class ServiceResult
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; } = string.Empty;
        public object? Data { get; set; }

        public static ServiceResult Success(string message = "Thao tác thành công", object? data = null)
        {
            return new ServiceResult { IsSuccess = true, Message = message, Data = data };
        }

        public static ServiceResult Failure(string message)
        {
            return new ServiceResult { IsSuccess = false, Message = message };
        }
    }

    public class ServiceResult<T> : ServiceResult
    {
        public new T? Data { get; set; }

        public static ServiceResult<T> Success(T data, string message = "Thao tác thành công")
        {
            return new ServiceResult<T> { IsSuccess = true, Message = message, Data = data };
        }

        public new static ServiceResult<T> Failure(string message)
        {
            return new ServiceResult<T> { IsSuccess = false, Message = message };
        }
    }
}
