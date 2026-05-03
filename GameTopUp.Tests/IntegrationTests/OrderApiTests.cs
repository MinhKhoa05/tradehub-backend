using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using GameTopUp.DAL.Entities;
using Xunit;
using Dapper;
using Microsoft.Extensions.DependencyInjection;
using GameTopUp.API;

namespace GameTopUp.Tests.IntegrationTests
{
    public class OrderApiTests : IClassFixture<CustomWebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;
        private readonly CustomWebApplicationFactory<Program> _factory;

        public OrderApiTests(CustomWebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
        }

        #region Seeding Helpers

        private async Task<long> SeedUserAsync(string username, string email)
        {
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<DAL.DatabaseContext>();
            var sql = @"INSERT INTO users (username, email, password_hash, is_active, created_at, updated_at) 
                        VALUES (@Username, @Email, 'hash', 1, @Now, @Now); 
                        SELECT last_insert_rowid();";
            return await db.Connection.QuerySingleAsync<long>(sql, new { Username = username, Email = email, Now = DateTime.UtcNow.ToString("O") });
        }

        private async Task SeedWalletAsync(long userId, decimal balance)
        {
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<DAL.DatabaseContext>();
            var sql = @"INSERT INTO wallets (user_id, balance, created_at, updated_at) 
                        VALUES (@UserId, @Balance, @Now, @Now);";
            await db.Connection.ExecuteAsync(sql, new { UserId = userId, Balance = balance, Now = DateTime.UtcNow.ToString("O") });
        }

        private async Task<long> SeedGameAsync(string name)
        {
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<DAL.DatabaseContext>();
            var sql = "INSERT INTO games (name, is_active) VALUES (@Name, 1); SELECT last_insert_rowid();";
            return await db.Connection.QuerySingleAsync<long>(sql, new { Name = name });
        }

        private async Task<long> SeedGamePackageAsync(long gameId, string name, decimal price)
        {
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<DAL.DatabaseContext>();
            var sql = @"INSERT INTO game_packages (name, game_id, normalized_name, sale_price, original_price, import_price, package_budget) 
                        VALUES (@Name, @GameId, @Normalized, @Price, @Price, @Price, 1000); 
                        SELECT last_insert_rowid();";
            return await db.Connection.QuerySingleAsync<long>(sql, new 
            { 
                Name = name, 
                GameId = gameId, 
                Normalized = name.ToLower(), 
                Price = price 
            });
        }

        private async Task<long> SeedOrderAsync(long userId, long packageId, decimal total, OrderStatus status)
        {
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<DAL.DatabaseContext>();
            // Sử dụng wallet_transaction_id = NULL để đơn giản hóa seeding nếu không test flow transaction cũ
            var sql = @"INSERT INTO orders (user_id, game_account_info, wallet_transaction_id, game_package_id, unit_price, quantity, status, created_at, updated_at) 
                        VALUES (@UserId, 'test_acc', NULL, @PackageId, @Price, 1, @Status, @Now, @Now); 
                        SELECT last_insert_rowid();";
            return await db.Connection.QuerySingleAsync<long>(sql, new 
            { 
                UserId = userId, 
                PackageId = packageId,
                Price = total, 
                Status = (int)status, 
                Now = DateTime.UtcNow.ToString("O") 
            });
        }

        private async Task<Order?> GetOrderFromDbAsync(long id)
        {
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<DAL.DatabaseContext>();
            return await db.Connection.QueryFirstOrDefaultAsync<Order>("SELECT * FROM orders WHERE id = @Id", new { Id = id });
        }

        private async Task<decimal> GetWalletBalanceAsync(long userId)
        {
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<DAL.DatabaseContext>();
            return await db.Connection.QuerySingleAsync<decimal>("SELECT balance FROM wallets WHERE user_id = @UserId", new { UserId = userId });
        }

        private async Task<int> GetOrderHistoryCountAsync(long orderId)
        {
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<DAL.DatabaseContext>();
            return await db.Connection.QuerySingleAsync<int>("SELECT COUNT(*) FROM order_history WHERE order_id = @OrderId", new { OrderId = orderId });
        }

        #endregion

        [Fact]
        public async Task PickOrder_ConcurrentRequests_OnlyOneShouldSucceed()
        {
            // Arrange
            var gameId = await SeedGameAsync("Test Game");
            var packageId = await SeedGamePackageAsync(gameId, "Test Package", 100);
            var customerId = await SeedUserAsync("customer_pick", "customer_pick@test.com");
            var orderId = await SeedOrderAsync(customerId, packageId, 100, OrderStatus.Pending);

            int concurrentRequests = 10;
            var tasks = new List<Task<HttpResponseMessage>>();

            // Act
            for (int i = 0; i < concurrentRequests; i++)
            {
                tasks.Add(_client.PostAsync($"/api/orders/{orderId}/pick", null));
            }

            var responses = await Task.WhenAll(tasks);

            // Assert
            var successCount = responses.Count(r => r.StatusCode == HttpStatusCode.OK);
            var failureCount = responses.Count(r => r.StatusCode == HttpStatusCode.BadRequest);

            // Chỉ có đúng 1 Admin nhận được đơn hàng
            successCount.Should().Be(1, "Exactly one request should succeed in picking the order");
            failureCount.Should().Be(concurrentRequests - 1, "All other requests should fail with 400");

            // Kiểm tra trạng thái cuối cùng trong DB
            var order = await GetOrderFromDbAsync(orderId);
            order!.Status.Should().Be(OrderStatus.Processing);
            order.AssignTo.Should().Be(1); // TestAuthHandler hardcoded ID 1
        }

        [Fact]
        public async Task CancelOrder_ConcurrentRequests_AllShouldReturnOk_ButOnlyOneRefund()
        {
            // Arrange
            var gameId = await SeedGameAsync("Test Game Cancel");
            var packageId = await SeedGamePackageAsync(gameId, "Test Package Cancel", 200);
            var customerId = await SeedUserAsync("customer_cancel", "customer_cancel@test.com");
            decimal initialBalance = 500;
            decimal orderTotal = 200;
            await SeedWalletAsync(customerId, initialBalance);
            var orderId = await SeedOrderAsync(customerId, packageId, orderTotal, OrderStatus.Pending);

            int concurrentRequests = 10;
            var tasks = new List<Task<HttpResponseMessage>>();

            // Act
            for (int i = 0; i < concurrentRequests; i++)
            {
                tasks.Add(_client.PostAsync($"/api/orders/{orderId}/cancel", null));
            }

            var responses = await Task.WhenAll(tasks);

            // Assert
            // Tính Idempotent: Tất cả đều trả về 200 OK
            foreach (var response in responses)
            {
                response.StatusCode.Should().Be(HttpStatusCode.OK);
            }

            // Kiểm tra Database: Số dư chỉ được hoàn 1 lần duy nhất
            var finalBalance = await GetWalletBalanceAsync(customerId);
            finalBalance.Should().Be(initialBalance + orderTotal);

            // Kiểm tra Database: Trạng thái đơn hàng là Cancelled
            var order = await GetOrderFromDbAsync(orderId);
            order!.Status.Should().Be(OrderStatus.Cancelled);

            // Kiểm tra History: Chỉ có 1 bản ghi log cho việc hủy
            var historyCount = await GetOrderHistoryCountAsync(orderId);
            historyCount.Should().Be(1);
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
