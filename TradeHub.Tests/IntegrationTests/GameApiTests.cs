using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using TradeHub.BLL.DTOs.Games;
using TradeHub.DAL.Entities;
using Xunit;
using TradeHub.API;

namespace TradeHub.Tests.IntegrationTests
{
    public class GameApiTests : IClassFixture<CustomWebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;
        private readonly CustomWebApplicationFactory<Program> _factory;

        public GameApiTests(CustomWebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
        }

        [Fact]
        public async Task GetAllGames_ShouldReturnWrappedOk()
        {
            // Act
            var response = await _client.GetAsync("/api/games");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<ApiResponseTestWrapper<List<Game>>>();
            result!.Success.Should().BeTrue();
            result.Data.Should().NotBeNull();
        }

        [Fact]
        public async Task GetGameById_ShouldReturnNotFound_WhenIdDoesNotExist()
        {
            // Act
            var response = await _client.GetAsync("/api/games/9999");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
            var result = await response.Content.ReadFromJsonAsync<ApiResponseTestWrapper<object>>();
            result!.Success.Should().BeFalse();
            result.Message.Should().Be("Game không tồn tại.");
        }

        [Fact]
        public async Task CreateGame_ShouldReturnCreated_WithCorrectData()
        {
            // Arrange
            var request = new CreateGameRequest 
            { 
                Name = "Genshin Impact", 
                ImageUrl = "genshin.png",
                IsActive = true
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/games", request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Created);
            var result = await response.Content.ReadFromJsonAsync<ApiResponseTestWrapper<Game>>();
            result!.Success.Should().BeTrue();
            result.Data.Should().NotBeNull();
            result.Data!.Name.Should().Be("Genshin Impact");
            result.Data.ImageUrl.Should().Be("genshin.png");
            result.Data.IsActive.Should().BeTrue();
        }

        [Fact]
        public async Task DeleteGame_ShouldPerformHardDelete_Successfully()
        {
            // Arrange - Create a game first
            var createResponse = await _client.PostAsJsonAsync("/api/games", new CreateGameRequest { Name = "Hard Delete Test" });
            var createdResult = await createResponse.Content.ReadFromJsonAsync<ApiResponseTestWrapper<Game>>();
            var gameId = createdResult!.Data!.Id;
            
            // Act - Delete it
            var response = await _client.DeleteAsync($"/api/games/{gameId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var deleteResult = await response.Content.ReadFromJsonAsync<ApiResponseTestWrapper<object>>();
            deleteResult!.Success.Should().BeTrue();
            deleteResult.Message.Should().Contain("Xóa Game thành công");

            // Verify it's really gone
            var getResponse = await _client.GetAsync($"/api/games/{gameId}");
            getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task CreateGame_ShouldReturnBadRequest_WhenNameIsEmpty()
        {
            // Arrange
            var request = new CreateGameRequest { Name = "", ImageUrl = "img.png" };

            // Act
            var response = await _client.PostAsJsonAsync("/api/games", request);

            // Assert
            // Rationale: Automatic validation by [ApiController] returns 400
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task CreateGame_ShouldHandleSpecialCharactersAndLongStrings()
        {
            // Arrange
            var longName = new string('A', 255); // Use 255 instead of 1000 to be more realistic but still long
            var specialChars = "!@#$%^&*()_+";
            var request = new CreateGameRequest { Name = specialChars + longName, ImageUrl = "img.png" };

            // Act
            var response = await _client.PostAsJsonAsync("/api/games", request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Created);
            var result = await response.Content.ReadFromJsonAsync<ApiResponseTestWrapper<Game>>();
            result!.Data!.Name.Should().Be(specialChars + longName);
        }

        private class ApiResponseTestWrapper<T>
        {
            public bool Success { get; set; }
            public T? Data { get; set; }
            public string? Message { get; set; }
        }
    }
}
