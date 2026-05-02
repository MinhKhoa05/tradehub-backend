using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using GameTopUp.BLL.DTOs.Users;
using GameTopUp.DAL.Entities;
using Xunit;
using Dapper;
using Microsoft.Extensions.DependencyInjection;
using GameTopUp.API;

namespace GameTopUp.Tests.IntegrationTests
{
    public class UserApiTests : IClassFixture<CustomWebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;
        private readonly CustomWebApplicationFactory<Program> _factory;

        public UserApiTests(CustomWebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
        }

        private async Task<long> SeedUserAsync(string username, string email, bool isActive = true)
        {
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<DAL.DatabaseContext>();
            var sql = @"INSERT INTO users (username, email, password_hash, is_active, created_at, updated_at) 
                        VALUES (@Username, @Email, 'hashed_pass', @IsActive, @Now, @Now); 
                        SELECT last_insert_rowid();";
            
            return await db.Connection.QuerySingleAsync<long>(sql, new 
            { 
                Username = username, 
                Email = email, 
                IsActive = isActive ? 1 : 0,
                Now = DateTime.UtcNow.ToString("O")
            });
        }

        private async Task<User?> GetUserFromDbAsync(long id)
        {
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<DAL.DatabaseContext>();
            return await db.Connection.QueryFirstOrDefaultAsync<User>("SELECT * FROM users WHERE id = @Id", new { Id = id });
        }

        [Fact]
        public async Task GetAllUsers_ShouldReturnWrappedList()
        {
            // Arrange
            await SeedUserAsync("user_list_1", "list1@test.com");
            await SeedUserAsync("user_list_2", "list2@test.com");

            // Act
            var response = await _client.GetAsync("/api/users");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<ApiResponseTestWrapper<List<UserResponseDTO>>>();
            result!.Success.Should().BeTrue();
            result.Data.Should().NotBeNull();
            result.Data!.Any(u => u.Username == "user_list_1").Should().BeTrue();
        }

        [Fact]
        public async Task GetUserById_ShouldReturnCorrectData_WhenUserExists()
        {
            // Arrange
            var id = await SeedUserAsync("integration_test_user", "integration@test.vn");

            // Act
            var response = await _client.GetAsync($"/api/users/{id}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<ApiResponseTestWrapper<UserResponseDTO>>();
            result!.Data.Should().NotBeNull();
            result.Data!.Id.Should().Be(id);
            result.Data.Username.Should().Be("integration_test_user");
            result.Data.Email.Should().Be("integration@test.vn");
        }

        [Fact]
        public async Task GetUserById_ShouldReturnNotFound_WhenUserDoesNotExist()
        {
            // Act
            var response = await _client.GetAsync("/api/users/9999");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
            var result = await response.Content.ReadFromJsonAsync<ApiResponseTestWrapper<object>>();
            result!.Success.Should().BeFalse();
            result.Message.Should().Be("Người dùng không tồn tại.");
        }

        [Fact]
        public async Task UpdateUser_ShouldActuallyUpdateDatabase()
        {
            // Arrange
            var id = await SeedUserAsync("original_name", "orig@test.com");
            var request = new UpdateUserRequest { Username = "updated_name" };

            // Act
            var response = await _client.PutAsJsonAsync($"/api/users/{id}", request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            
            var userInDb = await GetUserFromDbAsync(id);
            userInDb!.Username.Should().Be("updated_name");
        }

        [Fact]
        public async Task DeleteUser_ShouldPerformSoftDelete_BySettingIsActiveToFalse()
        {
            // Arrange
            var id = await SeedUserAsync("soft_delete_me", "soft@delete.com", isActive: true);

            // Act
            var response = await _client.DeleteAsync($"/api/users/{id}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            
            // Verify in Database
            var userInDb = await GetUserFromDbAsync(id);
            userInDb!.IsActive.Should().BeFalse();
        }

        // Helper class for deserializing ApiResponse in tests
        private class ApiResponseTestWrapper<T>
        {
            public bool Success { get; set; }
            public T? Data { get; set; }
            public string? Message { get; set; }
        }
    }
}
