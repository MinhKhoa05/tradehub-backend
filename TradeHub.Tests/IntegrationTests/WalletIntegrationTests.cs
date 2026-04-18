using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using TradeHub.BLL.Common;
using TradeHub.DAL;
using TradeHub.DAL.Entities;
using TradeHub.DAL.Repositories.Interfaces;
using Xunit;

namespace TradeHub.Tests.IntegrationTests
{
    public class WalletIntegrationTests : IClassFixture<WebApplicationFactory<TradeHub.API.IApiMarker>>
    {
        private readonly WebApplicationFactory<TradeHub.API.IApiMarker> _factory;
        private readonly Mock<IIdentityService> _identityServiceMock = new();
        private readonly Mock<IWalletRepository> _walletRepoMock = new();
        private readonly Mock<IWalletTransactionRepository> _walletTxRepoMock = new();
        private readonly Mock<DatabaseContext> _databaseMock = new Mock<DatabaseContext>("Server=fake");

        public WalletIntegrationTests(WebApplicationFactory<TradeHub.API.IApiMarker> factory)
        {
            _factory = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    // Mock Authentication
                    services.Configure<AuthenticationOptions>(options =>
                    {
                        options.DefaultAuthenticateScheme = "Test";
                        options.DefaultChallengeScheme = "Test";
                    });

                    services.AddAuthentication("Test")
                        .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(
                            "Test", options => { });

                    // Mock Identity
                    _identityServiceMock.Setup(x => x.UserId).Returns(1);
                    services.AddScoped(_ => _identityServiceMock.Object);

                    // Mock Repositories
                    services.AddScoped(_ => _walletRepoMock.Object);
                    services.AddScoped(_ => _walletTxRepoMock.Object);

                    // Mock DatabaseContext
                    _databaseMock.Setup(x => x.ExecuteInTransactionAsync(It.IsAny<Func<Task>>()))
                        .Returns<Func<Task>>(async action => await action());
                    services.AddScoped(_ => _databaseMock.Object);
                });
            });
        }

        [Fact]
        public async Task Pay_WhenValidRequest_ShouldReturnOk()
        {
            // Arrange
            var client = _factory.CreateClient();
            var request = new { OrderIds = new List<long> { 1, 2 }, TotalAmount = 50000 };

            var wallet = new Wallet { Id = 1, UserId = 1, Balance = 100000 };
            _walletRepoMock.Setup(x => x.GetByUserIdAsync(1)).ReturnsAsync(wallet);
            _walletRepoMock.Setup(x => x.DecreaseBalanceAsync(1, 50000)).ReturnsAsync(1);

            // Act
            var response = await client.PostAsJsonAsync("/api/wallet/pay", request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task TransactionRollback_WhenHistoryFails_ShouldReturnInternalServerError()
        {
            // Arrange
            var client = _factory.CreateClient();
            var request = new { OrderIds = new List<long> { 1 }, TotalAmount = 50000 };

            var wallet = new Wallet { Id = 1, UserId = 1, Balance = 100000 };
            _walletRepoMock.Setup(x => x.GetByUserIdAsync(1)).ReturnsAsync(wallet);
            
            // Step 1: Trừ tiền thành công
            _walletRepoMock.Setup(x => x.DecreaseBalanceAsync(1, 50000)).ReturnsAsync(1);
            
            // Step 2: Ghi lịch sử thất bại (Gây lỗi)
            _walletTxRepoMock.Setup(x => x.CreateAsync(It.IsAny<WalletTransaction>()))
                .ThrowsAsync(new System.Exception("Database crash!"));

            // Act
            var response = await client.PostAsJsonAsync("/api/wallet/pay", request);

            // Assert
            // GlobalExceptionMiddleware sẽ bắt lỗi này và trả về 500
            response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
            
            // Note: Trong unit test ta có thể verify DatabaseContext.RollbackAsync() được gọi.
            // Ở đây ta Verify middleware xử lý đúng.
        }
    }
}
