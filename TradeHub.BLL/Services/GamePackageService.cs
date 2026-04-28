using TradeHub.BLL.Common;
using TradeHub.BLL.DTOs.GamePackages;
using TradeHub.BLL.Exceptions;
using TradeHub.BLL.Utils;
using TradeHub.DAL.Entities;
using TradeHub.DAL.Repositories.Interfaces;

namespace TradeHub.BLL.Services
{
    public class GamePackageService : BaseService
    {
        private readonly IGamePackageRepository _packageRepo;
        private readonly IGameRepository _gameRepo;

        public GamePackageService(IGamePackageRepository packageRepo, IGameRepository gameRepo, IIdentityService identityService)
            : base(identityService)
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
            return await _packageRepo.GetByIdAsync(id)
                ?? throw new BusinessException("Game Package không tồn tại");
        }

        public async Task<GamePackage> CreatePackageAsync(CreateGamePackageRequest request)
        {
            // Verify game exists
            var game = await _gameRepo.GetByIdAsync(request.GameId) 
                ?? throw new BusinessException("Game không tồn tại");

            if (!game.IsActive)
            {
                throw new BusinessException("Không thể thêm Package vào Game đang bị khóa (Inactive)");
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

            if (request.Name != null)
            {
                package.Name = request.Name;
                package.NormalizedName = NormalizeName.Normalize(request.Name);
            }
            
            if (request.ImageUrl != null) package.ImageUrl = request.ImageUrl;
            if (request.SalePrice.HasValue) package.SalePrice = request.SalePrice.Value;
            if (request.OriginalPrice.HasValue) package.OriginalPrice = request.OriginalPrice.Value;
            if (request.ImportPrice.HasValue) package.ImportPrice = request.ImportPrice.Value;
            if (request.PackageBudget.HasValue) package.PackageBudget = request.PackageBudget.Value;
            if (request.IsActive.HasValue) package.IsActive = request.IsActive.Value;

            await _packageRepo.UpdateAsync(package);
            return package;
        }

        public async Task DeletePackageAsync(long id)
        {
            await GetPackageByIdAsync(id); // Check exists
            await _packageRepo.DeleteAsync(id);
        }
    }
}