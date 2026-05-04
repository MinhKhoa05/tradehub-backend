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

namespace GameTopUp.Tests.UnitTests.Services
{
    public class OrderUseCaseTests
    {
        private readonly Mock<IOrderRepository> _orderRepoMock;
        private readonly Mock<IOrderHistoryRepository> _orderHistoryRepoMock;
        private readonly Mock<IWalletRepository> _walletRepoMock;
        private readonly Mock<IWalletTransactionRepository> _walletTxRepoMock;
        private readonly Mock<DatabaseContext> _dbMock;
        
        private readonly OrderService _orderService;
        private readonly WalletService _walletService;
        private readonly OrderUseCase _orderUseCase;

        public OrderUseCaseTests()
        {
            _orderRepoMock = new Mock<IOrderRepository>();
            _orderHistoryRepoMock = new Mock<IOrderHistoryRepository>();
            _walletRepoMock = new Mock<IWalletRepository>();
            _walletTxRepoMock = new Mock<IWalletTransactionRepository>();
            
            _dbMock = new Mock<DatabaseContext>(Mock.Of<System.Data.Common.DbConnection>());
            _dbMock.Setup(d => d.ExecuteInTransactionAsync(It.IsAny<Func<Task>>()))
                .Returns(async (Func<Task> action) => await action());

            _dbMock.Setup(d => d.ExecuteInTransactionAsync(It.IsAny<Func<Task<BLL.DTOs.Orders.CheckoutResponseDTO>>>()))
                .Returns(async (Func<Task<BLL.DTOs.Orders.CheckoutResponseDTO>> action) => await action());

            _dbMock.Setup(d => d.ExecuteInTransactionAsync(It.IsAny<Func<Task<BLL.DTOs.Wallets.TransactionResponseDTO>>>()))
                .Returns(async (Func<Task<BLL.DTOs.Wallets.TransactionResponseDTO>> action) => await action());

            _orderService = new OrderService(_orderRepoMock.Object, _orderHistoryRepoMock.Object);
            _walletService = new WalletService(_walletRepoMock.Object, _walletTxRepoMock.Object, _dbMock.Object);

            _orderUseCase = new OrderUseCase(null!, _walletService, _orderService, _dbMock.Object);
        }

        [Fact]
        public async Task PickOrderAsync_ShouldSucceed()
        {
            // Arrange
            long orderId = 123;
            var adminContext = new UserContext(1, "admin", "Admin");
            var order = new Order { Id = orderId, Status = OrderStatus.Pending };

            _orderRepoMock.Setup(r => r.GetByIdForUpdateAsync(orderId)).ReturnsAsync(order);

            // Act
            await _orderUseCase.PickOrderAsync(orderId, adminContext);

            // Assert
            _orderRepoMock.Verify(r => r.UpdateAsync(It.Is<Order>(o => o.Status == OrderStatus.Processing)), Times.Once);
            _orderHistoryRepoMock.Verify(r => r.CreateAsync(It.IsAny<OrderHistory>()), Times.Once);
        }

        [Fact]
        public async Task CompleteOrderAsync_ShouldSucceed()
        {
            // Arrange
            long orderId = 123;
            var adminContext = new UserContext(1, "admin", "Admin");
            var order = new Order { Id = orderId, Status = OrderStatus.Processing, AssignTo = adminContext.UserId };

            _orderRepoMock.Setup(r => r.GetByIdForUpdateAsync(orderId)).ReturnsAsync(order);

            // Act
            await _orderUseCase.CompleteOrderAsync(orderId, adminContext);

            // Assert
            _orderRepoMock.Verify(r => r.UpdateAsync(It.Is<Order>(o => o.Status == OrderStatus.Completed)), Times.Once);
            _orderHistoryRepoMock.Verify(r => r.CreateAsync(It.IsAny<OrderHistory>()), Times.Once);
        }

        [Fact]
        public async Task CancelOrderAsync_ShouldSucceed_WhenOrderIsPending()
        {
            // Arrange
            long orderId = 123;
            long userId = 456;
            var adminContext = new UserContext(1, "admin", "Admin");
            var order = new Order { Id = orderId, UserId = userId, Status = OrderStatus.Pending, UnitPrice = 100, Quantity = 1 };

            _orderRepoMock.Setup(r => r.GetByIdForUpdateAsync(orderId)).ReturnsAsync(order);
            
            _walletRepoMock.Setup(r => r.GetByUserIdAsync(userId)).ReturnsAsync(new Wallet { UserId = userId, Balance = 500 });
            _walletRepoMock.Setup(r => r.IncreaseBalanceAsync(userId, order.Total)).ReturnsAsync(1);

            // Act
            await _orderUseCase.CancelOrderAsync(orderId, adminContext);

            // Assert
            _orderRepoMock.Verify(r => r.UpdateAsync(It.Is<Order>(o => o.Status == OrderStatus.Cancelled)), Times.Once);
            _walletRepoMock.Verify(r => r.IncreaseBalanceAsync(userId, order.Total), Times.Once);
            _orderHistoryRepoMock.Verify(r => r.CreateAsync(It.Is<OrderHistory>(h => h.ToStatus == OrderStatus.Cancelled)), Times.Once);
        }

        [Fact]
        public async Task CancelOrderAsync_ShouldBeIdempotent_WhenOrderIsAlreadyCancelled()
        {
            // Arrange
            long orderId = 123;
            var adminContext = new UserContext(1, "admin", "Admin");
            var order = new Order { Id = orderId, Status = OrderStatus.Cancelled };

            _orderRepoMock.Setup(r => r.GetByIdForUpdateAsync(orderId)).ReturnsAsync(order);

            // Act
            await _orderUseCase.CancelOrderAsync(orderId, adminContext);

            // Assert
            _orderRepoMock.Verify(r => r.UpdateAsync(It.IsAny<Order>()), Times.Never);
            _walletRepoMock.Verify(r => r.IncreaseBalanceAsync(It.IsAny<long>(), It.IsAny<decimal>()), Times.Never);
        }

        [Fact]
        public async Task CancelOrderAsync_ShouldThrowBusinessException_WhenOrderIsProcessing()
        {
            // Arrange
            long orderId = 123;
            var adminContext = new UserContext(1, "admin", "Admin");
            var order = new Order { Id = orderId, Status = OrderStatus.Processing };

            _orderRepoMock.Setup(r => r.GetByIdForUpdateAsync(orderId)).ReturnsAsync(order);

            // Act
            Func<Task> act = () => _orderUseCase.CancelOrderAsync(orderId, adminContext);

            // Assert
            await act.Should().ThrowAsync<BusinessException>()
                .WithMessage("Không thể hủy đơn hàng đang ở trạng thái này.");
        }
    }
}
