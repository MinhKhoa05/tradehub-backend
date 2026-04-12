using TradeHub.BLL.Common;
using System.Security.Authentication;

namespace TradeHub.BLL.Services
{
    public abstract class BaseService
    {
        protected readonly IIdentityService _identityService;

        protected BaseService(IIdentityService identityService)
        {
            _identityService = identityService;
        }

        /// <summary>
        /// Lấy UserId của người dùng hiện tại. 
        /// Nếu chưa đăng nhập (UserId null), văng lỗi Unauthorized ngay lập tức.
        /// </summary>
        protected long CurrentUserId => _identityService.UserId
            ?? throw new AuthenticationException("Phiên đăng nhập không hợp lệ hoặc đã hết hạn.");

        /// <summary>
        /// Kiểm tra xem người dùng đã đăng nhập hay chưa (trả về true/false)
        /// </summary>
        protected bool IsAuthenticated => _identityService.UserId.HasValue;
    }
}