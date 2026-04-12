using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using TradeHub.BLL.DTOs.Auths;
using TradeHub.BLL.DTOs.Users;
using TradeHub.BLL.ApplicationServices;

namespace TradeHub.API.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : BaseController
    {
        private readonly AuthService _auth;
        
        public AuthController(AuthService auth)
        {
            _auth = auth;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(CreateUserRequest registerRequest)
        {
            await _auth.RegisterAsync(registerRequest);
            return ApiCreated();
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequest loginRequest)
        {
            string token = await _auth.LoginAsync(loginRequest);

            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,        // Quan trọng nhất: JavaScript không thể đọc được cookie này (Chống XSS)
                Secure = false,         // Để false vì đang chạy HTTP local (Khi lên Production có HTTPS thì để true)
                SameSite = SameSiteMode.Lax, // Chống tấn công CSRF
                Expires = DateTime.UtcNow.AddDays(7) // Thời gian sống của Cookie
            };

            // 3. Ghi Cookie vào Response
            // Tên cookie ông đặt là gì cũng được, ở đây tui ví dụ là "accessToken"
            Response.Cookies.Append("accessToken", token, cookieOptions);

            // 4. Trả về kết quả (Vẫn trả về token trong body nếu ông muốn React dùng song song)
            return ApiOk(new { accessToken = token });
        }

        [Authorize]
        [HttpPut("password")]
        public async Task<IActionResult> ChangePassword(PasswordChangeRequest passwordChangeRequest)
        {
            await _auth.ChangePasswordAsync(passwordChangeRequest);
            return ApiNoContent();
        }
    }
}
