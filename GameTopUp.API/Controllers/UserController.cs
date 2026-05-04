using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GameTopUp.BLL.DTOs.Users;
using GameTopUp.BLL.Services;

namespace GameTopUp.API.Controllers
{
    [Route("api/users")]
    public class UserController : ApiControllerBase
    {
        private readonly UserService _userService;

        public UserController(UserService userService)
        {
            _userService = userService;
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var result = await _userService.GetAllAsync(page, pageSize);
            return ApiOk(result);
        }

        [Authorize]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(long id)
        {
            var result = await _userService.GetByIdAsync(id);
            return ApiOk(result);
        }

        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> GetMyProfile()
        {
            var result = await _userService.GetByIdAsync(CurrentUser.UserId);
            return ApiOk(result);
        }

        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(long id, [FromBody] UpdateUserRequest request)
        {
            await _userService.UpdateProfileAsync(id, request);
            return ApiOk(message: "Cập nhật thông tin thành công.");
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(long id)
        {
            await _userService.DeleteAsync(id);
            return ApiOk(message: "Xóa người dùng thành công.");
        }
    }
}
