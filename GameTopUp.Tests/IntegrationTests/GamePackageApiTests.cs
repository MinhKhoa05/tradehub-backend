using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using GameTopUp.BLL.DTOs.GamePackages;
using GameTopUp.BLL.DTOs.Games;
using GameTopUp.DAL.Entities;
using Xunit;
using GameTopUp.API;

namespace GameTopUp.Tests.IntegrationTests
{
    [Collection("IntegrationTests")]
    public class GamePackageApiTests : IAsyncLifetime
    {
        private readonly HttpClient _client;
        private readonly CustomWebApplicationFactory<Program> _factory;

        public GamePackageApiTests(CustomWebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
        }

        public async Task InitializeAsync()
        {
            await _factory.ResetDatabaseAsync();
        }

        public Task DisposeAsync()
        {
            return Task.CompletedTask;
        }

        private async Task<long> CreateGameAsync(string name, bool isActive = true)
        {
            var request = new CreateGameRequest { Name = name, IsActive = isActive };
            var response = await _client.PostAsJsonAsync("/api/games", request);
            var result = await response.Content.ReadFromJsonAsync<ApiResponseTestWrapper<Game>>();
            return result!.Data!.Id;
        }

        [Fact]
        public async Task CreatePackage_ShouldReturnCreated_WhenDataIsValid()
        {
            // Arrange
            var gameId = await CreateGameAsync("Active Game For Package");
            var request = new CreateGamePackageRequest 
            { 
                GameId = gameId, 
                Name = "Gói 100K", 
                SalePrice = 100000,
                ImportPrice = 70000,
                IsActive = true
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/game-packages", request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Created);
            var result = await response.Content.ReadFromJsonAsync<ApiResponseTestWrapper<GamePackage>>();
            result!.Success.Should().BeTrue();
            result.Data!.Name.Should().Be("Gói 100K");
            result.Data.NormalizedName.Should().Be("goi 100k");
        }

        [Fact]
        public async Task CreatePackage_ShouldReturnBadRequest_WhenGameIsInactive()
        {
            // Arrange
            var gameId = await CreateGameAsync("Inactive Game", isActive: false);
            var request = new CreateGamePackageRequest 
            { 
                GameId = gameId, 
                Name = "Pack on Inactive Game", 
                SalePrice = 100 
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/game-packages", request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            var result = await response.Content.ReadFromJsonAsync<ApiResponseTestWrapper<object>>();
            result!.Success.Should().BeFalse();
            result.Message.Should().Contain("ngừng hoạt động");
        }

        [Fact]
        public async Task GetPackagesByGameId_ShouldReturnOnlyActivePackages()
        {
            // Arrange
            var gameId = await CreateGameAsync("Game with many packages");
            
            // Create 1 active package
            await _client.PostAsJsonAsync("/api/game-packages", new CreateGamePackageRequest 
            { 
                GameId = gameId, Name = "Active Pack", SalePrice = 100, IsActive = true 
            });
            
            // Create 1 inactive package
            await _client.PostAsJsonAsync("/api/game-packages", new CreateGamePackageRequest 
            { 
                GameId = gameId, Name = "Inactive Pack", SalePrice = 100, IsActive = false 
            });

            // Act
            var response = await _client.GetAsync($"/api/game-packages/game/{gameId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<ApiResponseTestWrapper<List<GamePackage>>>();
            result!.Data.Should().HaveCount(1);
            result.Data!.All(p => p.IsActive).Should().BeTrue();
            result.Data.First().Name.Should().Be("Active Pack");
        }

        [Fact]
        public async Task DeletePackage_ShouldPerformHardDelete()
        {
            // Arrange
            var gameId = await CreateGameAsync("Game for Delete Pack");
            var createResponse = await _client.PostAsJsonAsync("/api/game-packages", new CreateGamePackageRequest 
            { 
                GameId = gameId, Name = "To Be Deleted", SalePrice = 100 
            });
            var createdResult = await createResponse.Content.ReadFromJsonAsync<ApiResponseTestWrapper<GamePackage>>();
            var packageId = createdResult!.Data!.Id;

            // Act
            var response = await _client.DeleteAsync($"/api/game-packages/{packageId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            
            // Verify hard delete by trying to get it
            var getResponse = await _client.GetAsync($"/api/game-packages/{packageId}");
            getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

    }
}
