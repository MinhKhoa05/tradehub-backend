using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using TradeHub.BLL.DTOs.Auths;
using TradeHub.BLL.DTOs.Users;
using TradeHub.BLL.ApplicationServices;

namespace TradeHub.API.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ApiControllerBase
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
            return ApiCreated(null, "Đăng ký tài khoản thành công.");
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequest loginRequest)
        {
            string token = await _auth.LoginAsync(loginRequest);

            // Cấu hình Cookie để tăng tính bảo mật (HttpOnly)
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = false, // Nên bật True nếu chạy trên HTTPS
                SameSite = SameSiteMode.Lax,
                Expires = DateTime.UtcNow.AddDays(7)
            };

            if (!Response.HasStarted)
            {
                Response.Cookies.Append("accessToken", token, cookieOptions);
            }

            return ApiOk(new { accessToken = token }, "Đăng nhập thành công.");
        }

        [Authorize]
        [HttpPut("password")]
        public async Task<IActionResult> ChangePassword(PasswordChangeRequest passwordChangeRequest)
        {
            await _auth.ChangePasswordAsync(CurrentUser, passwordChangeRequest);
            return ApiNoContent();
        }
    }
}
