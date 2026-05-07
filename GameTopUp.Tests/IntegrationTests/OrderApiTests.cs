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
            var sql = @"INSERT INTO game_packages (name, game_id, normalized_name, sale_price, original_price, import_price) 
                        VALUES (@Name, @GameId, @Normalized, @Price, @Price, @Price); 
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
            var sql = @"INSERT INTO orders (user_id, game_account_info, game_package_id, unit_price, quantity, status, created_at, updated_at) 
                        VALUES (@UserId, 'test_acc', @PackageId, @Price, 1, @Status, @Now, @Now); 
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

        private async Task UpdatePackageStockAsync(long packageId, int stock)
        {
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<DAL.DatabaseContext>();
            var sql = "UPDATE game_packages SET stock_quantity = @Stock WHERE id = @Id";
            await db.Connection.ExecuteAsync(sql, new { Stock = stock, Id = packageId });
        }

        private async Task UpdatePackageStatusAsync(long packageId, bool isActive)
        {
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<DAL.DatabaseContext>();
            var sql = "UPDATE game_packages SET is_active = @IsActive WHERE id = @Id";
            await db.Connection.ExecuteAsync(sql, new { IsActive = isActive ? 1 : 0, Id = packageId });
        }

        private async Task<int> GetPackageStockAsync(long packageId)
        {
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<DAL.DatabaseContext>();
            return await db.Connection.QuerySingleAsync<int>("SELECT stock_quantity FROM game_packages WHERE id = @Id", new { Id = packageId });
        }

        #endregion

        [Fact(Skip = "SQLite does not support 'FOR UPDATE'. Migration to Testcontainers MySQL is planned to verify Race Conditions.")]
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

        [Fact(Skip = "SQLite does not support 'FOR UPDATE'. Migration to Testcontainers MySQL is planned to verify Race Conditions.")]
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

        [Fact(Skip = "SQLite does not support 'FOR UPDATE' locking. Planned for MySQL Testcontainers.")]
        public async Task CompleteOrder_ShouldSucceed_WhenAdminCompletesAssignedOrder()
        {
            // Arrange
            var gameId = await SeedGameAsync("Game Complete");
            var packageId = await SeedGamePackageAsync(gameId, "Pkg Complete", 100);
            var customerId = await SeedUserAsync("cust_comp", "cust_comp@test.com");
            // Đơn hàng phải được THANH TOÁN (Paid) thì Admin mới Pick được
            var orderId = await SeedOrderAsync(customerId, packageId, 100, OrderStatus.Paid);

            // Seed Admin để đảm bảo ID tồn tại trong DB cho FK AssignTo
            var adminId = await SeedUserAsync("admin_comp", "admin_comp@test.com");

            // Admin picks the order first
            var pickRequest = new HttpRequestMessage(HttpMethod.Post, $"/api/orders/{orderId}/pick");
            pickRequest.Headers.Add("X-Test-UserId", adminId.ToString());
            pickRequest.Headers.Add("X-Test-Role", "Admin");
            var pickResponse = await _client.SendAsync(pickRequest);
            pickResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            // Act
            var completeRequest = new HttpRequestMessage(HttpMethod.Post, $"/api/orders/{orderId}/complete");
            completeRequest.Headers.Add("X-Test-UserId", adminId.ToString());
            completeRequest.Headers.Add("X-Test-Role", "Admin");
            var response = await _client.SendAsync(completeRequest);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            
            var order = await GetOrderFromDbAsync(orderId);
            order!.Status.Should().Be(OrderStatus.Completed);

            var historyCount = await GetOrderHistoryCountAsync(orderId);
            historyCount.Should().Be(2); // 1 for Pick, 1 for Complete
        }

        [Fact(Skip = "SQLite does not support 'FOR UPDATE'. Migration to Testcontainers MySQL is planned to verify Race Conditions.")]
        public async Task CompleteOrder_ConcurrentRequests_ShouldBeIdempotent()
        {
            // Arrange
            var gameId = await SeedGameAsync("Game Comp Race");
            var packageId = await SeedGamePackageAsync(gameId, "Pkg Comp Race", 100);
            var customerId = await SeedUserAsync("cust_race", "cust_race@test.com");
            // Đơn hàng phải được THANH TOÁN (Paid) thì Admin mới Pick được
            var orderId = await SeedOrderAsync(customerId, packageId, 100, OrderStatus.Paid);

            // Admin picks the order
            await _client.PostAsync($"/api/orders/{orderId}/pick", null);

            int concurrentRequests = 10;
            var tasks = new List<Task<HttpResponseMessage>>();

            // Act
            for (int i = 0; i < concurrentRequests; i++)
            {
                tasks.Add(_client.PostAsync($"/api/orders/{orderId}/complete", null));
            }

            var responses = await Task.WhenAll(tasks);

            // Assert
            foreach (var response in responses)
            {
                response.StatusCode.Should().Be(HttpStatusCode.OK);
            }

            var order = await GetOrderFromDbAsync(orderId);
            order!.Status.Should().Be(OrderStatus.Completed);

            // History should only contain 1 Pick and 1 Complete log
            var historyCount = await GetOrderHistoryCountAsync(orderId);
            historyCount.Should().Be(2);
        }
        
        #region PlaceOrder Tests

        [Fact]
        public async Task PlaceOrder_HappyPath_ShouldCreateOrderAndDecreaseStock()
        {
            // Arrange
            var gameId = await SeedGameAsync("Happy Game");
            var packageId = await SeedGamePackageAsync(gameId, "Happy Pkg", 100);
            await UpdatePackageStockAsync(packageId, 10);
            
            // Seed User mới để đảm bảo không bị trùng Pending Order từ các test case khác
            var customerId = await SeedUserAsync("happy_customer", "happy@test.com");
            
            var request = new { GamePackageId = packageId, Quantity = 2, GameAccountInfo = "player_123" };

            // Act
            var httpRequest = new HttpRequestMessage(HttpMethod.Post, "api/orders/place")
            {
                Content = JsonContent.Create(request)
            };
            httpRequest.Headers.Add("X-Test-UserId", customerId.ToString());
            httpRequest.Headers.Add("X-Test-Role", "Customer");

            var response = await _client.SendAsync(httpRequest);
            
            response.StatusCode.Should().Be(HttpStatusCode.Created);

            var result = await response.Content.ReadFromJsonAsync<ApiResponseTestWrapper<long>>();
            var orderId = result!.Data;
            
            // Assert
            // USER_TASK: Kiểm tra response status là OK hoặc Created.
            // Kiểm tra trong DB xem đơn hàng đã được tạo chưa (status Pending, UserId, Quantity...).
            // Kiểm tra xem Stock của package có giảm đi đúng 2 đơn vị không.
            // // TODO: USER IMPLEMENT
            var order = await GetOrderFromDbAsync(orderId);
            order!.Status.Should().Be(OrderStatus.Pending);

            var stockQuantity = await GetPackageStockAsync(packageId);
            stockQuantity.Should().Be(8); // Ban đầu là 10, mua 2, kiểm tra còn 8.
        }

        [Fact]
        public async Task PlaceOrder_InsufficientStock_ShouldReturnBadRequest()
        {
            // Arrange
            var gameId = await SeedGameAsync("Stock Game");
            var packageId = await SeedGamePackageAsync(gameId, "Low Stock Pkg", 100);
            await UpdatePackageStockAsync(packageId, 1); // Chỉ còn 1
            
            var request = new { GamePackageId = packageId, Quantity = 5, GameAccountInfo = "player_456" };

            // Act
            // USER_TASK: Gọi API đặt hàng với số lượng vượt quá tồn kho.
            // // TODO: USER IMPLEMENT
            var response = await _client.PostAsJsonAsync("api/orders/place", request);

            // Assert
            // USER_TASK: Verify status code là BadRequest (400).
            // Verify Stock trong DB không bị thay đổi (vẫn là 1).
            // // TODO: USER IMPLEMENT
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            var stockQuantity = await GetPackageStockAsync(packageId);
            stockQuantity.Should().Be(1); // Stock không thay đổi.
        }

        [Fact]
        public async Task PlaceOrder_InactivePackage_ShouldReturnBadRequest()
        {
            // Arrange
            var gameId = await SeedGameAsync("Inactive Game");
            var packageId = await SeedGamePackageAsync(gameId, "Inactive Pkg", 100);
            await UpdatePackageStatusAsync(packageId, false);
            
            var request = new { GamePackageId = packageId, Quantity = 1, GameAccountInfo = "player_inactive" };

            // Act
            // USER_TASK: Gọi API đặt hàng với gói game đã bị disable.
            // // TODO: USER IMPLEMENT
            var response = await _client.PostAsJsonAsync("api/orders/place", request);

            // Assert
            // USER_TASK: Verify status code là BadRequest (400).
            // // TODO: USER IMPLEMENT
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task PlaceOrder_OnePendingOrderLimit_ShouldReturnBadRequest()
        {
            // Arrange
            var gameId = await SeedGameAsync("Limit Game");
            var packageId = await SeedGamePackageAsync(gameId, "Limit Pkg", 100);
            
            // Seed User để tránh FK error
            var customerId = await SeedUserAsync("limit_user", "limit@test.com");
            
            // Giả lập đã có 1 đơn hàng Pending
            await SeedOrderAsync(customerId, packageId, 100, OrderStatus.Pending);
            
            var request = new { GamePackageId = packageId, Quantity = 1, GameAccountInfo = "player_limit" };

            // Act
            // USER_TASK: Gọi API đặt hàng lần thứ hai khi đơn trước chưa xử lý/thanh toán.
            // // TODO: USER IMPLEMENT
            var response = await _client.PostAsJsonAsync("api/orders/place", request);

            // Assert
            // USER_TASK: Verify status code là BadRequest (400) do vi phạm rule "mỗi user chỉ có 1 đơn Pending".
            // // TODO: USER IMPLEMENT
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact(Skip = "SQLite limitations with concurrent writes/locks. Planned for MySQL Testcontainers.")]
        public async Task PlaceOrder_Concurrent_ShouldNotExceedStock()
        {
            // Arrange
            var gameId = await SeedGameAsync("Race Game");
            var packageId = await SeedGamePackageAsync(gameId, "Race Pkg", 100);
            await UpdatePackageStockAsync(packageId, 5); // Chỉ có 5 item
            
            int concurrentRequests = 10;
            var request = new { GamePackageId = packageId, Quantity = 1, GameAccountInfo = "race_player" };

            // Tạo 10 User khác nhau để bypass rule "mỗi user chỉ có 1 đơn Pending"
            var userIds = new List<long>();
            for (int i = 1; i <= concurrentRequests; i++)
            {
                userIds.Add(await SeedUserAsync($"race_user_{i}", $"race_{i}@test.com"));
            }

            // Act
            var tasks = new List<Task<HttpResponseMessage>>();
            foreach (var userId in userIds)
            {
                var httpRequest = new HttpRequestMessage(HttpMethod.Post, "api/orders/place")
                {
                    Content = JsonContent.Create(request)
                };
                httpRequest.Headers.Add("X-Test-UserId", userId.ToString());
                httpRequest.Headers.Add("X-Test-Role", "Customer");
                
                tasks.Add(_client.SendAsync(httpRequest));
            }

            var responses = await Task.WhenAll(tasks);

            // Assert

            var stockQuantity = await GetPackageStockAsync(packageId);
            stockQuantity.Should().Be(0);

            var successCount = responses.Count(r => r.StatusCode == HttpStatusCode.Created);
            successCount.Should().Be(5);
            
            var failedCount = responses.Count(r => r.StatusCode == HttpStatusCode.BadRequest);
            failedCount.Should().Be(5);
        }

        #endregion
    }
}

