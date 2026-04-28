using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TradeHub.BLL.DTOs.Games;
using TradeHub.BLL.Services;

namespace TradeHub.API.Controllers
{
    [Route("api/games")]
    [ApiController]
    public class GameController : BaseController
    {
        private readonly GameService _gameService;

        public GameController(GameService gameService)
        {
            _gameService = gameService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllGames()
        {
            var games = await _gameService.GetAllGamesAsync();
            return ApiOk(games);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetGameById(long id)
        {
            var game = await _gameService.GetGameByIdAsync(id);
            return ApiOk(game);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> CreateGame([FromBody] CreateGameRequest request)
        {
            var game = await _gameService.CreateGameAsync(request);
            return ApiCreated(game, "Tạo Game thành công");
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateGame(long id, [FromBody] UpdateGameRequest request)
        {
            var game = await _gameService.UpdateGameAsync(id, request);
            return ApiOk(game, "Cập nhật Game thành công");
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteGame(long id)
        {
            await _gameService.DeleteGameAsync(id);
            return ApiOk(null, "Xóa Game thành công");
        }
    }
}
