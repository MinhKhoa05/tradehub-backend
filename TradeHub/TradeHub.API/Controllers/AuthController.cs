using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using TradeHub.BLL.Services;
using TradeHub.BLL.Mappings;
using TradeHub.BLL.DTOs.Auths;
using TradeHub.BLL.DTOs.Users;

namespace TradeHub.API.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;

        public AuthController(AuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(CreateUserRequest registerRequest)
        {
            await _authService.RegisterAsync(registerRequest);
            return Ok();
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequest loginRequest)
        {
            string token = await _authService.LoginAsync(loginRequest);
            return Ok( new { accessToken = token });
        }

        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> GetMe()
        {
            int userId = HttpContext.GetUserId();
            var user = await _authService.GetMe(userId);
            return Ok(user?.ToResponse());
        }
        
        [Authorize]
        [HttpPut("change-password")]
        public async Task<IActionResult> ChangePassword(PasswordChangeRequest passwordChangeRequest)
        {
            int userId = HttpContext.GetUserId();
            await _authService.ChangePasswordAsync(userId, passwordChangeRequest);
            return Ok();
        }
    }
}
