using GameTopUp.BLL.DTOs.GamePackages;
using GameTopUp.BLL.Exceptions;
using GameTopUp.BLL.Utils;
using GameTopUp.DAL.Entities;
using GameTopUp.DAL.Interfaces;
using Mapster;

namespace GameTopUp.BLL.Services
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
                StockQuantity = request.StockQuantity,
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

        public async Task IncreaseStockAsync(long id, int quantity)
        {
            ValidateStockQuantity(quantity);

            await _packageRepo.IncreaseStockAsync(id, quantity);
        }

        public async Task DecreaseStockAsync(long id, int quantity)
        {
            ValidateStockQuantity(quantity);

            var affectedRows = await _packageRepo.DecreaseStockAsync(id, quantity);
            if (affectedRows == 0) throw new BusinessException("Không đủ số lượng trong kho.");            
        }

        private void ValidateStockQuantity(int quantity)
        {
            if (quantity <= 0) throw new BusinessException("Số lượng phải lớn hơn 0.");
        }
    }
}
