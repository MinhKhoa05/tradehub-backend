using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TradeHub.BLL.DTOs.GamePackages;
using TradeHub.BLL.Services;

namespace TradeHub.API.Controllers
{
    [Route("api/game-packages")]
    [ApiController]
    public class GamePackageController : BaseController
    {
        private readonly GamePackageService _packageService;

        public GamePackageController(GamePackageService packageService)
        {
            _packageService = packageService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllPackages()
        {
            var packages = await _packageService.GetAllPackagesAsync();
            return ApiOk(packages);
        }

        [HttpGet("game/{gameId}")]
        public async Task<IActionResult> GetPackagesByGameId(long gameId)
        {
            var packages = await _packageService.GetPackagesByGameIdAsync(gameId);
            return ApiOk(packages);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetPackageById(long id)
        {
            var package = await _packageService.GetPackageByIdAsync(id);
            return ApiOk(package);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> CreatePackage([FromBody] CreateGamePackageRequest request)
        {
            var package = await _packageService.CreatePackageAsync(request);
            return ApiCreated(package, "Tạo Game Package thành công");
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePackage(long id, [FromBody] UpdateGamePackageRequest request)
        {
            var package = await _packageService.UpdatePackageAsync(id, request);
            return ApiOk(package, "Cập nhật Game Package thành công");
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePackage(long id)
        {
            await _packageService.DeletePackageAsync(id);
            return ApiOk(null, "Xóa Game Package thành công");
        }
    }
}
