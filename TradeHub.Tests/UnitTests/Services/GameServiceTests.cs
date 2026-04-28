using Moq;
using TradeHub.BLL.DTOs.Games;
using TradeHub.BLL.Exceptions;
using TradeHub.BLL.Services;
using TradeHub.DAL.Entities;
using TradeHub.DAL.Repositories.Interfaces;
using Xunit;
using FluentAssertions;

namespace TradeHub.Tests.UnitTests.Services
{
    public class GameServiceTests
    {
        private readonly Mock<IGameRepository> _gameRepoMock;
        private readonly GameService _gameService;

        public GameServiceTests()
        {
            _gameRepoMock = new Mock<IGameRepository>();
            _gameService = new GameService(_gameRepoMock.Object);
        }

        [Fact]
        public async Task CreateGameAsync_ShouldReturnGame_WhenValidRequest()
        {
            // Arrange
            var request = new CreateGameRequest { Name = "Genshin Impact", ImageUrl = "genshin.png", IsActive = true };
            _gameRepoMock.Setup(r => r.CreateAsync(It.IsAny<Game>())).ReturnsAsync(1L);

            // Act
            var result = await _gameService.CreateGameAsync(request);

            // Assert
            result.Should().NotBeNull();
            result.Name.Should().Be(request.Name);
            result.Id.Should().Be(1L);
            _gameRepoMock.Verify(r => r.CreateAsync(It.IsAny<Game>()), Times.Once);
        }

        [Fact]
        public async Task GetGameByIdAsync_ShouldThrowException_WhenGameNotFound()
        {
            // Arrange
            _gameRepoMock.Setup(r => r.GetByIdAsync(1L)).ReturnsAsync((Game?)null);

            // Act
            Func<Task> act = () => _gameService.GetGameByIdAsync(1L);

            // Assert
            await act.Should().ThrowAsync<BusinessException>().WithMessage("Game không tồn tại");
        }

        [Fact]
        public async Task UpdateGameAsync_ShouldUpdateFields_WhenGameExists()
        {
            // Arrange
            var existingGame = new Game { Id = 1L, Name = "Old Name", IsActive = true };
            var request = new UpdateGameRequest { Name = "New Name", IsActive = false };
            
            _gameRepoMock.Setup(r => r.GetByIdAsync(1L)).ReturnsAsync(existingGame);

            // Act
            var result = await _gameService.UpdateGameAsync(1L, request);

            // Assert
            result.Name.Should().Be("New Name");
            result.IsActive.Should().BeFalse();
            _gameRepoMock.Verify(r => r.UpdateAsync(It.IsAny<Game>()), Times.Once);
        }

        [Fact]
        public async Task DeleteGameAsync_ShouldCallDelete_WhenGameExists()
        {
            // Arrange
            var existingGame = new Game { Id = 1L, Name = "To Delete" };
            _gameRepoMock.Setup(r => r.GetByIdAsync(1L)).ReturnsAsync(existingGame);

            // Act
            await _gameService.DeleteGameAsync(1L);

            // Assert
            _gameRepoMock.Verify(r => r.DeleteAsync(1L), Times.Once);
        }
    }
}
