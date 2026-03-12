using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using TradeHub.BLL.Mappings;
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
            return ApiOk( new { accessToken = token });
        }

        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> GetMe()
        {
            var user = await _auth.GetCurrentUserAsync(CurrentUserId);
            return ApiOk(user.ToResponse());
        }
        
        [Authorize]
        [HttpPut("password")]
        public async Task<IActionResult> ChangePassword(PasswordChangeRequest passwordChangeRequest)
        {
            await _auth.ChangePasswordAsync(CurrentUserId, passwordChangeRequest);
            return ApiNoContent();
        }
    }
}
