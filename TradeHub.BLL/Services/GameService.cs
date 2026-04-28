using TradeHub.BLL.DTOs.Games;
using TradeHub.BLL.Exceptions;
using TradeHub.DAL.Entities;
using TradeHub.DAL.Repositories.Interfaces;

namespace TradeHub.BLL.Services
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
            return await _gameRepo.GetByIdAsync(id) 
                ?? throw new BusinessException("Game không tồn tại");
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

            if (request.Name != null) game.Name = request.Name;
            if (request.ImageUrl != null) game.ImageUrl = request.ImageUrl;
            if (request.IsActive.HasValue) game.IsActive = request.IsActive.Value;

            await _gameRepo.UpdateAsync(game);
            return game;
        }

        public async Task DeleteGameAsync(long id)
        {
            await GetGameByIdAsync(id); // Check exists
            await _gameRepo.DeleteAsync(id);
        }
    }
}
