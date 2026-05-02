
namespace GameTopUp.BLL.Exceptions
{
    /// <summary>
    /// Ngoại lệ ném ra khi yêu cầu xác thực không thành công hoặc token hết hạn (401 Unauthorized).
    /// </summary>
    public class UnauthorizedException : BusinessException
    {
        public UnauthorizedException(string message = "Phiên làm việc đã hết hạn hoặc không hợp lệ.")
            : base(message)
        {
        }
    }
}
