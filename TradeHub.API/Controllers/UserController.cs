using Microsoft.AspNetCore.Mvc;
using TradeHub.BLL.DTOs.Users;
using TradeHub.BLL.Services;

namespace TradeHub.API.Controllers
{
    [Route("api/users")]
    public class UserController : ApiControllerBase
    {
        private readonly UserService _userService;

        public UserController(UserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var result = await _userService.GetAllAsync(page, pageSize);
            return ApiOk(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(long id)
        {
            var result = await _userService.GetByIdAsync(id);
            return ApiOk(result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(long id, [FromBody] UpdateUserRequest request)
        {
            await _userService.UpdateProfileAsync(id, request);
            return ApiOk(message: "Cập nhật thông tin thành công.");
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(long id)
        {
            await _userService.DeleteAsync(id);
            return ApiOk(message: "Xóa người dùng thành công.");
        }
    }
}
