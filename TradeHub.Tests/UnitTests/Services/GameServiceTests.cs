using Moq;
using TradeHub.BLL.DTOs.Games;
using TradeHub.BLL.Exceptions;
using TradeHub.BLL.Services;
using TradeHub.DAL.Entities;
using TradeHub.DAL.Interfaces;
using Xunit;
using FluentAssertions;
using TradeHub.BLL.Config;

namespace TradeHub.Tests.UnitTests.Services
{
    public class GameServiceTests
    {
        private readonly Mock<IGameRepository> _gameRepoMock;
        private readonly GameService _gameService;

        public GameServiceTests()
        {
            MapsterConfig.RegisterMappings();
            _gameRepoMock = new Mock<IGameRepository>();
            _gameService = new GameService(_gameRepoMock.Object);
        }

        [Fact]
        public async Task CreateGameAsync_ShouldCreateWithFullData_WhenRequestIsValid()
        {
            // Arrange
            var request = new CreateGameRequest 
            { 
                Name = "Liên Quân Mobile", 
                ImageUrl = "lienquan.png", 
                IsActive = true 
            };
            _gameRepoMock.Setup(r => r.CreateAsync(It.IsAny<Game>())).ReturnsAsync(88L);

            // Act
            var result = await _gameService.CreateGameAsync(request);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(88L);
            result.Name.Should().Be("Liên Quân Mobile");
            result.ImageUrl.Should().Be("lienquan.png");
            
            _gameRepoMock.Verify(r => r.CreateAsync(It.Is<Game>(g => 
                g.Name == "Liên Quân Mobile" && 
                g.ImageUrl == "lienquan.png")), Times.Once);
        }

        [Fact]
        public async Task GetGameByIdAsync_ShouldThrowNotFound_WhenGameDoesNotExist()
        {
            // Arrange
            _gameRepoMock.Setup(r => r.GetByIdAsync(123L)).ReturnsAsync((Game?)null);

            // Act
            Func<Task> act = () => _gameService.GetGameByIdAsync(123L);

            // Assert
            await act.Should().ThrowAsync<NotFoundException>()
                .WithMessage("Game không tồn tại.");
        }

        [Fact]
        public async Task UpdateGameAsync_ShouldApplyPartialUpdates_WhenUsingMapster()
        {
            // Arrange
            var existingGame = new Game { Id = 1L, Name = "Old Game", ImageUrl = "old.png", IsActive = true };
            var request = new UpdateGameRequest { Name = "Updated Game", IsActive = false }; // ImageUrl is null
            
            _gameRepoMock.Setup(r => r.GetByIdAsync(1L)).ReturnsAsync(existingGame);

            // Act
            var result = await _gameService.UpdateGameAsync(1L, request);

            // Assert
            result.Name.Should().Be("Updated Game");
            result.ImageUrl.Should().Be("old.png"); // Should NOT be overwritten by null
            result.IsActive.Should().BeFalse();
            
            _gameRepoMock.Verify(r => r.UpdateAsync(It.IsAny<Game>()), Times.Once);
        }

        [Fact]
        public async Task DeleteGameAsync_ShouldPerformHardDelete_ByCallingRepoDelete()
        {
            // Arrange
            var existingGame = new Game { Id = 99L, Name = "Game to Delete" };
            _gameRepoMock.Setup(r => r.GetByIdAsync(99L)).ReturnsAsync(existingGame);

            // Act
            await _gameService.DeleteGameAsync(99L);

            // Assert
            // Rationale: We check existence first, then call hard delete as per GameRepository implementation
            _gameRepoMock.Verify(r => r.GetByIdAsync(99L), Times.Once);
            _gameRepoMock.Verify(r => r.DeleteAsync(99L), Times.Once);
        }
    }
}
