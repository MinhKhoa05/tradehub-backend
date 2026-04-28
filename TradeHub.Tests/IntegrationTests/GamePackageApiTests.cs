using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using TradeHub.BLL.DTOs.GamePackages;
using TradeHub.BLL.DTOs.Games;
using TradeHub.DAL.Entities;
using Xunit;

namespace TradeHub.Tests.IntegrationTests
{
    public class GamePackageApiTests : IClassFixture<CustomWebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;
        private readonly CustomWebApplicationFactory<Program> _factory;

        public GamePackageApiTests(CustomWebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
        }

        [Fact]
        public async Task CreatePackage_ShouldReturnBadRequest_WhenGameIsInactive()
        {
            // 1. Create an inactive game
            var gameRequest = new CreateGameRequest { Name = "Inactive Game", IsActive = false };
            var gameResponse = await _client.PostAsJsonAsync("/api/games", gameRequest);
            var game = await gameResponse.Content.ReadFromJsonAsync<Game>();

            // 2. Try to add package
            var packageRequest = new CreateGamePackageRequest 
            { 
                GameId = game!.Id, 
                Name = "Pack 1", 
                SalePrice = 100 
            };
            var response = await _client.PostAsJsonAsync("/api/game-packages", packageRequest);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task CreatePackage_ShouldReturnBadRequest_WhenPriceIsNegative()
        {
            // Arrange
            var request = new CreateGamePackageRequest 
            { 
                GameId = 1, 
                Name = "Bad Pack", 
                SalePrice = -10 
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/game-packages", request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }
    }
}
