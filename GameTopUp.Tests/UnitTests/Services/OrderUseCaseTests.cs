using Moq;
using GameTopUp.BLL.ApplicationServices;
using GameTopUp.BLL.Services;
using GameTopUp.BLL.Common;
using GameTopUp.BLL.Exceptions;
using GameTopUp.DAL.Entities;
using GameTopUp.DAL.Interfaces;
using GameTopUp.DAL;
using Xunit;
using FluentAssertions;
using GameTopUp.BLL.DTOs.Orders;

namespace GameTopUp.Tests.UnitTests.Services
{
    public class OrderUseCaseTests
    {
        private readonly Mock<IOrderRepository> _orderRepoMock;
        private readonly Mock<IOrderHistoryRepository> _orderHistoryRepoMock;
        private readonly Mock<IWalletRepository> _walletRepoMock;
        private readonly Mock<IWalletTransactionRepository> _walletTxRepoMock;
        private readonly Mock<IGamePackageRepository> _packageRepoMock;
        private readonly Mock<DatabaseContext> _dbMock;
        
        private readonly OrderService _orderService;
        private readonly WalletService _walletService;
        private readonly GamePackageService _packageService;
        private readonly OrderUseCase _orderUseCase;

        public OrderUseCaseTests()
        {
            _orderRepoMock = new Mock<IOrderRepository>();
            _orderHistoryRepoMock = new Mock<IOrderHistoryRepository>();
            _walletRepoMock = new Mock<IWalletRepository>();
            _walletTxRepoMock = new Mock<IWalletTransactionRepository>();
            _packageRepoMock = new Mock<IGamePackageRepository>();
            
            _dbMock = new Mock<DatabaseContext>(Mock.Of<System.Data.Common.DbConnection>());
            _dbMock.Setup(d => d.ExecuteInTransactionAsync(It.IsAny<Func<Task>>()))
                .Returns(async (Func<Task> action) => await action());

            _dbMock.Setup(d => d.ExecuteInTransactionAsync(It.IsAny<Func<Task<long>>>()))
                .Returns(async (Func<Task<long>> action) => await action());

            _orderService = new OrderService(_orderRepoMock.Object, _orderHistoryRepoMock.Object);
            _walletService = new WalletService(_walletRepoMock.Object, _walletTxRepoMock.Object, _dbMock.Object);
            _packageService = new GamePackageService(_packageRepoMock.Object);

            _orderUseCase = new OrderUseCase(_packageService, _walletService, _orderService, _dbMock.Object);
        }

        [Fact]
        public async Task PlaceOrderAsync_ShouldSucceed()
        {
            // Arrange
            var context = new UserContext(1, "user", "Customer");
            var request = new PlaceOrderRequestDTO { GamePackageId = 10, Quantity = 2, GameAccountInfo = "Acc1" };
            var package = new GamePackage { Id = 10, SalePrice = 50, IsActive = true, StockQuantity = 10 };

            _packageRepoMock.Setup(r => r.GetByIdAsync(10)).ReturnsAsync(package);
            _orderRepoMock.Setup(r => r.HasPendingOrderAsync(context.UserId)).ReturnsAsync(false);
            _orderRepoMock.Setup(r => r.CreateAsync(It.IsAny<Order>())).ReturnsAsync(999);

            // Act
            var result = await _orderUseCase.PlaceOrderAsync(context, request);

            // Assert
            result.Should().Be(999);
            _packageRepoMock.Verify(r => r.DecreaseStockAsync(10, 2), Times.Once);
            _orderRepoMock.Verify(r => r.CreateAsync(It.Is<Order>(o => o.UnitPrice == 50 && o.Status == OrderStatus.Pending)), Times.Once);
        }

        [Fact]
        public async Task PayOrderAsync_ShouldSucceed()
        {
            // Arrange
            var context = new UserContext(1, "user", "Customer");
            long orderId = 123;
            var order = new Order { Id = orderId, UserId = context.UserId, Status = OrderStatus.Pending, UnitPrice = 100, Quantity = 1 };

            _orderRepoMock.Setup(r => r.GetByIdForUpdateAsync(orderId)).ReturnsAsync(order);
            _walletRepoMock.Setup(r => r.GetByUserIdAsync(context.UserId)).ReturnsAsync(new Wallet { UserId = context.UserId, Balance = 500 });
            _walletRepoMock.Setup(r => r.DecreaseBalanceAsync(context.UserId, 100)).ReturnsAsync(1);
            _walletTxRepoMock.Setup(r => r.CreateAsync(It.IsAny<WalletTransaction>())).ReturnsAsync(1);

            // Act
            await _orderUseCase.PayOrderAsync(context, orderId);

            // Assert
            _orderRepoMock.Verify(r => r.UpdateAsync(It.Is<Order>(o => o.Status == OrderStatus.Paid)), Times.Once);
            _walletRepoMock.Verify(r => r.DecreaseBalanceAsync(context.UserId, 100), Times.Once);
        }

        [Fact]
        public async Task PickOrderAsync_ShouldSucceed()
        {
            // Arrange
            long orderId = 123;
            var adminContext = new UserContext(2, "admin", "Admin") { IsAdmin = true };
            var order = new Order { Id = orderId, Status = OrderStatus.Paid };

            _orderRepoMock.Setup(r => r.GetByIdForUpdateAsync(orderId)).ReturnsAsync(order);

            // Act
            await _orderUseCase.PickOrderAsync(orderId, adminContext);

            // Assert
            _orderRepoMock.Verify(r => r.UpdateAsync(It.Is<Order>(o => o.Status == OrderStatus.Processing)), Times.Once);
            _orderHistoryRepoMock.Verify(r => r.CreateAsync(It.IsAny<OrderHistory>()), Times.Once);
        }

        [Fact]
        public async Task CancelOrderAsync_ShouldSucceed_WhenOrderIsPaid()
        {
            // Arrange
            long orderId = 123;
            long userId = 456;
            var adminContext = new UserContext(2, "admin", "Admin") { IsAdmin = true };
            var order = new Order { Id = orderId, UserId = userId, Status = OrderStatus.Paid, UnitPrice = 100, Quantity = 1, GamePackageId = 10 };

            _orderRepoMock.Setup(r => r.GetByIdForUpdateAsync(orderId)).ReturnsAsync(order);
            _walletRepoMock.Setup(r => r.GetByUserIdAsync(userId)).ReturnsAsync(new Wallet { UserId = userId, Balance = 500 });
            
            // Act
            await _orderUseCase.CancelOrderAsync(orderId, adminContext);

            // Assert
            _orderRepoMock.Verify(r => r.UpdateAsync(It.Is<Order>(o => o.Status == OrderStatus.Cancelled)), Times.Once);
            _packageRepoMock.Verify(r => r.IncreaseStockAsync(10, 1), Times.Once); // Restock
            _walletRepoMock.Verify(r => r.IncreaseBalanceAsync(userId, 100), Times.Once); // Refund
        }

        [Fact]
        public async Task CancelOrderAsync_ShouldOnlyRestock_WhenOrderIsPending()
        {
            // Arrange
            long orderId = 123;
            long userId = 456;
            var adminContext = new UserContext(2, "admin", "Admin") { IsAdmin = true };
            var order = new Order { Id = orderId, UserId = userId, Status = OrderStatus.Pending, UnitPrice = 100, Quantity = 1, GamePackageId = 10 };

            _orderRepoMock.Setup(r => r.GetByIdForUpdateAsync(orderId)).ReturnsAsync(order);

            // Act
            await _orderUseCase.CancelOrderAsync(orderId, adminContext);

            // Assert
            _orderRepoMock.Verify(r => r.UpdateAsync(It.Is<Order>(o => o.Status == OrderStatus.Cancelled)), Times.Once);
            _packageRepoMock.Verify(r => r.IncreaseStockAsync(10, 1), Times.Once); // Restock
            _walletRepoMock.Verify(r => r.IncreaseBalanceAsync(It.IsAny<long>(), It.IsAny<decimal>()), Times.Never); // No refund
        }
    }
}
