using Moq;
using TradeHub.BLL.DTOs.GamePackages;
using TradeHub.BLL.Exceptions;
using TradeHub.BLL.Services;
using TradeHub.DAL.Entities;
using TradeHub.DAL.Repositories.Interfaces;
using Xunit;
using FluentAssertions;

namespace TradeHub.Tests.UnitTests.Services
{
    public class GamePackageServiceTests
    {
        private readonly Mock<IGamePackageRepository> _packageRepoMock;
        private readonly Mock<IGameRepository> _gameRepoMock;
        private readonly GamePackageService _packageService;

        public GamePackageServiceTests()
        {
            _packageRepoMock = new Mock<IGamePackageRepository>();
            _gameRepoMock = new Mock<IGameRepository>();
            _packageService = new GamePackageService(_packageRepoMock.Object, _gameRepoMock.Object);
        }

        [Fact]
        public async Task CreatePackageAsync_ShouldThrowException_WhenGameIsInactive()
        {
            // Arrange
            var inactiveGame = new Game { Id = 1L, Name = "Inactive Game", IsActive = false };
            var request = new CreateGamePackageRequest { GameId = 1L, Name = "Test Package" };
            
            _gameRepoMock.Setup(r => r.GetByIdAsync(1L)).ReturnsAsync(inactiveGame);

            // Act
            Func<Task> act = () => _packageService.CreatePackageAsync(request);

            // Assert
            await act.Should().ThrowAsync<BusinessException>()
                .WithMessage("Không thể thêm Package vào Game đang bị khóa (Inactive)");
        }

        [Fact]
        public async Task CreatePackageAsync_ShouldReturnPackage_WhenValid()
        {
            // Arrange
            var activeGame = new Game { Id = 1L, Name = "Active Game", IsActive = true };
            var request = new CreateGamePackageRequest 
            { 
                GameId = 1L, 
                Name = "Premium Pack", 
                SalePrice = 100, 
                OriginalPrice = 120, 
                ImportPrice = 80, 
                PackageBudget = 1000 
            };
            
            _gameRepoMock.Setup(r => r.GetByIdAsync(1L)).ReturnsAsync(activeGame);
            _packageRepoMock.Setup(r => r.CreateAsync(It.IsAny<GamePackage>())).ReturnsAsync(10L);

            // Act
            var result = await _packageService.CreatePackageAsync(request);

            // Assert
            result.Should().NotBeNull();
            result.Name.Should().Be(request.Name);
            result.GameId.Should().Be(1L);
            result.SalePrice.Should().Be(100);
            _packageRepoMock.Verify(r => r.CreateAsync(It.IsAny<GamePackage>()), Times.Once);
        }

        [Fact]
        public async Task GetPackageByIdAsync_ShouldThrowException_WhenNotFound()
        {
            // Arrange
            _packageRepoMock.Setup(r => r.GetByIdAsync(99L)).ReturnsAsync((GamePackage?)null);

            // Act
            Func<Task> act = () => _packageService.GetPackageByIdAsync(99L);

            // Assert
            await act.Should().ThrowAsync<BusinessException>().WithMessage("Game Package không tồn tại");
        }
    }
}
