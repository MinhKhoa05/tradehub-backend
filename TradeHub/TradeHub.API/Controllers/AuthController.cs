using Microsoft.AspNetCore.Mvc;
using TradeHub.BLL.Services;
using TradeHub.BLL.DTOs;
using Microsoft.AspNetCore.Authorization;

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
        public async Task<IActionResult> Register([FromBody] UserRegisterRequest registerRequest)
        {
            await _authService.Register(registerRequest);
            return Ok();
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest loginRequest)
        {
            string token = await _authService.Login(loginRequest);
            return Ok( new { accessToken = token });
        }

        [HttpGet("me")]
        public async Task<IActionResult> GetMe()
        {
            int userId = HttpContext.GetUserId();
            var user = await _authService.GetMe(userId);
            return Ok(user);
        }
        
        [Authorize]
        [HttpPut("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] PasswordChangeRequest passwordChangeRequest)
        {
            int userId = HttpContext.GetUserId();
            await _authService.ChangePassword(userId, passwordChangeRequest);
            return Ok();
        }
    }
}
