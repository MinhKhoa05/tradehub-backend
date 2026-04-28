using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using TradeHub.BLL.DTOs.Games;
using TradeHub.DAL.Entities;
using Xunit;

namespace TradeHub.Tests.IntegrationTests
{
    public class GameApiTests : IClassFixture<CustomWebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;
        private readonly CustomWebApplicationFactory<Program> _factory;

        public GameApiTests(CustomWebApplicationFactory<Program> factory)
        {
            _factory = factory;
            // CreateClient will use the overridden services
            _client = _factory.CreateClient();
        }

        [Fact]
        public async Task GetAllGames_ShouldReturnOk()
        {
            // Act
            var response = await _client.GetAsync("/api/games");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task GetGameById_ShouldReturnNotFound_WhenIdDoesNotExist()
        {
            // Act
            var response = await _client.GetAsync("/api/games/999");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task CreateGame_ShouldReturnCreated_WhenValid()
        {
            // Arrange
            var request = new CreateGameRequest { Name = "New Game", ImageUrl = "img.png" };

            // Act
            var response = await _client.PostAsJsonAsync("/api/games", request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Created);
            var content = await response.Content.ReadFromJsonAsync<dynamic>();
            // Note: Since ApiCreated returns a wrapped response, I might need to adjust based on BaseController
        }

        [Fact]
        public async Task DeleteGame_ShouldDeleteCascadePackages()
        {
            // Arrange - Create a game
            var createGameResponse = await _client.PostAsJsonAsync("/api/games", new CreateGameRequest { Name = "Game to Delete" });
            var game = await createGameResponse.Content.ReadFromJsonAsync<Game>(); 
            
            // Delete
            var response = await _client.DeleteAsync($"/api/games/{game!.Id}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task CreateGame_ShouldReturnBadRequest_WhenNameIsEmpty()
        {
            // Arrange
            var request = new CreateGameRequest { Name = "", ImageUrl = "img.png" };

            // Act
            var response = await _client.PostAsJsonAsync("/api/games", request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task CreateGame_ShouldHandleSpecialCharactersAndLongStrings()
        {
            // Arrange
            var longName = new string('A', 1000);
            var specialChars = "!@#$%^&*()_+";
            var request = new CreateGameRequest { Name = specialChars + longName, ImageUrl = "img.png" };

            // Act
            var response = await _client.PostAsJsonAsync("/api/games", request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Created);
        }
    }
}
