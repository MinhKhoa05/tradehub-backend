
namespace GameTopUp.BLL.Exceptions
{
    /// <summary>
    /// Ngoại lệ cơ bản cho các lỗi nghiệp vụ trong hệ thống.
    /// Khi ném ngoại lệ này, Middleware sẽ tự động bắt lại và trả về lỗi 400 Bad Request cho Client.
    /// </summary>
    public class BusinessException : Exception
    {
        public BusinessException(string message = "")
            : base(message)
        {
        }
    }
}
