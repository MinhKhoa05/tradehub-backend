using System.Security.Claims;
using TradeHub.BLL.Common;

namespace TradeHub.API
{
    public class IdentityService : IIdentityService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public IdentityService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public long? UserId
        {
            get
            {
                // Tìm Claim chứa ID (thường là NameIdentifier trong JWT)
                var userIdClaim = _httpContextAccessor.HttpContext?.User?
                    .FindFirst(ClaimTypes.NameIdentifier)?.Value;

                return long.TryParse(userIdClaim, out var id) ? id : null;
            }
        }
    }
}