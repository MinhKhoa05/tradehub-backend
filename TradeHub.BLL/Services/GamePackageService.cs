using TradeHub.BLL.DTOs.GamePackages;
using TradeHub.BLL.Exceptions;
using TradeHub.BLL.Utils;
using TradeHub.DAL.Entities;
using TradeHub.DAL.Interfaces;
using Mapster;

namespace TradeHub.BLL.Services
{
    public class GamePackageService
    {
        private readonly IGamePackageRepository _packageRepo;
        private readonly IGameRepository _gameRepo;

        public GamePackageService(IGamePackageRepository packageRepo, IGameRepository gameRepo)
        {
            _packageRepo = packageRepo;
            _gameRepo = gameRepo;
        }

        public async Task<List<GamePackage>> GetAllPackagesAsync()
        {
            return await _packageRepo.GetAllAsync();
        }

        public async Task<List<GamePackage>> GetPackagesByGameIdAsync(long gameId)
        {
            return await _packageRepo.GetByGameIdAsync(gameId);
        }

        public async Task<GamePackage> GetPackageByIdAsync(long id)
        {
            var package = await _packageRepo.GetByIdAsync(id);
            if (package == null)
            {
                throw new NotFoundException("Gói nạp không tồn tại.");
            }
            return package;
        }

        public async Task<GamePackage> CreatePackageAsync(CreateGamePackageRequest request)
        {
            var game = await _gameRepo.GetByIdAsync(request.GameId);
            if (game == null)
            {
                throw new NotFoundException("Game không tồn tại.");
            }

            if (!game.IsActive)
            {
                throw new BusinessException("Không thể thêm gói nạp vào Game đang ở trạng thái ngừng hoạt động.");
            }

            var package = new GamePackage
            {
                Name = request.Name,
                NormalizedName = NormalizeName.Normalize(request.Name),
                ImageUrl = request.ImageUrl,
                GameId = request.GameId,
                SalePrice = request.SalePrice,
                OriginalPrice = request.OriginalPrice,
                ImportPrice = request.ImportPrice,
                PackageBudget = request.PackageBudget,
                SpentAmount = 0,
                IsActive = request.IsActive
            };

            package.Id = await _packageRepo.CreateAsync(package);
            return package;
        }

        public async Task<GamePackage> UpdatePackageAsync(long id, UpdateGamePackageRequest request)
        {
            var package = await GetPackageByIdAsync(id);
            request.Adapt(package);
            await _packageRepo.UpdateAsync(package);
            return package;
        }

        public async Task DeletePackageAsync(long id)
        {
            await GetPackageByIdAsync(id);
            await _packageRepo.DeleteAsync(id);
        }
    }
}
