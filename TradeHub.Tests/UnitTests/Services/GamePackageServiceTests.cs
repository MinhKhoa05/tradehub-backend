using Moq;
using TradeHub.BLL.DTOs.GamePackages;
using TradeHub.BLL.Exceptions;
using TradeHub.BLL.Services;
using TradeHub.DAL.Entities;
using TradeHub.DAL.Repositories.Interfaces;
using Xunit;
using FluentAssertions;
using TradeHub.BLL.Config;

namespace TradeHub.Tests.UnitTests.Services
{
    public class GamePackageServiceTests
    {
        private readonly Mock<IGamePackageRepository> _packageRepoMock;
        private readonly Mock<IGameRepository> _gameRepoMock;
        private readonly GamePackageService _packageService;

        public GamePackageServiceTests()
        {
            MapsterConfig.RegisterMappings();
            _packageRepoMock = new Mock<IGamePackageRepository>();
            _gameRepoMock = new Mock<IGameRepository>();
            _packageService = new GamePackageService(_packageRepoMock.Object, _gameRepoMock.Object);
        }

        [Fact]
        public async Task CreatePackageAsync_ShouldCreate_WhenGameIsValid()
        {
            // Arrange
            var game = new Game { Id = 1, IsActive = true };
            var request = new CreateGamePackageRequest 
            { 
                GameId = 1, 
                Name = "999 Kim Cương", 
                SalePrice = 150000, 
                ImportPrice = 100000,
                PackageBudget = 1000000
            };
            
            _gameRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(game);
            _packageRepoMock.Setup(r => r.CreateAsync(It.IsAny<GamePackage>())).ReturnsAsync(500L);

            // Act
            var result = await _packageService.CreatePackageAsync(request);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(500L);
            result.Name.Should().Be("999 Kim Cương");
            result.NormalizedName.Should().Be("999 kim cuong"); // Rationale: Verifying normalization logic
            
            _packageRepoMock.Verify(r => r.CreateAsync(It.IsAny<GamePackage>()), Times.Once);
        }

        [Fact]
        public async Task CreatePackageAsync_ShouldThrow_WhenGameIsInactive()
        {
            // Arrange
            var game = new Game { Id = 1, IsActive = false };
            var request = new CreateGamePackageRequest { GameId = 1 };
            _gameRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(game);

            // Act
            Func<Task> act = () => _packageService.CreatePackageAsync(request);

            // Assert
            await act.Should().ThrowAsync<BusinessException>()
                .WithMessage("Không thể thêm gói nạp vào Game đang ở trạng thái ngừng hoạt động.");
        }

        [Fact]
        public async Task GetPackageByIdAsync_ShouldThrowNotFound_WhenDoesNotExist()
        {
            // Arrange
            _packageRepoMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((GamePackage?)null);

            // Act
            Func<Task> act = () => _packageService.GetPackageByIdAsync(99);

            // Assert
            await act.Should().ThrowAsync<NotFoundException>()
                .WithMessage("Gói nạp không tồn tại.");
        }

        [Fact]
        public async Task UpdatePackageAsync_ShouldUseMapster_ForPartialUpdate()
        {
            // Arrange
            var existing = new GamePackage { Id = 1, Name = "Old", SalePrice = 100 };
            var request = new UpdateGamePackageRequest { Name = "New" }; // SalePrice is null/default in DTO if not provided? 
            // In C# decimal is not nullable unless specified. But DTO might have nullable.
            
            _packageRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(existing);

            // Act
            await _packageService.UpdatePackageAsync(1, request);

            // Assert
            existing.Name.Should().Be("New");
            _packageRepoMock.Verify(r => r.UpdateAsync(existing), Times.Once);
        }

        [Fact]
        public async Task DeletePackageAsync_ShouldPerformHardDelete()
        {
            // Arrange
            var existing = new GamePackage { Id = 1 };
            _packageRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(existing);

            // Act
            await _packageService.DeletePackageAsync(1);

            // Assert
            _packageRepoMock.Verify(r => r.DeleteAsync(1), Times.Once);
        }
    }
}
