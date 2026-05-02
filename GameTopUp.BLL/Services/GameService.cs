using GameTopUp.BLL.DTOs.Games;
using GameTopUp.BLL.Exceptions;
using GameTopUp.DAL.Entities;
using GameTopUp.DAL.Interfaces;
using Mapster;

namespace GameTopUp.BLL.Services
{
    public class GameService
    {
        private readonly IGameRepository _gameRepo;

        public GameService(IGameRepository gameRepo)
        {
            _gameRepo = gameRepo;
        }

        public async Task<List<Game>> GetAllGamesAsync()
        {
            return await _gameRepo.GetAllAsync();
        }

        public async Task<Game> GetGameByIdAsync(long id)
        {
            var game = await _gameRepo.GetByIdAsync(id) ?? throw new NotFoundException("Game không tồn tại.");
            return game;
        }

        public async Task<Game> CreateGameAsync(CreateGameRequest request)
        {
            var game = new Game 
            { 
                Name = request.Name, 
                ImageUrl = request.ImageUrl, 
                IsActive = request.IsActive 
            };
            
            game.Id = await _gameRepo.CreateAsync(game);
            return game;
        }

        public async Task<Game> UpdateGameAsync(long id, UpdateGameRequest request)
        {
            var game = await GetGameByIdAsync(id);
            request.Adapt(game);
            await _gameRepo.UpdateAsync(game);
            return game;
        }

        public async Task DeleteGameAsync(long id)
        {
            // Kiểm tra tồn tại trước khi xóa để đưa ra lỗi nghiệp vụ rõ ràng
            await GetGameByIdAsync(id);
            await _gameRepo.DeleteAsync(id);
        }
    }
}
