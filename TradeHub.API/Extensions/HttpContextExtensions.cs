using System.Security.Claims;

namespace TradeHub.API.Extensions
{
    public static class HttpContextExtensions
    {
        public static int GetUserId(this HttpContext context)
        {
            var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
                throw new UnauthorizedAccessException("UserId không có trong token");

            return int.Parse(userId);
        }
    }
}
