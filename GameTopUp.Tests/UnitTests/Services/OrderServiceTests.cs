using Moq;
using GameTopUp.BLL.Services;
using GameTopUp.DAL.Interfaces;
using GameTopUp.DAL;
using GameTopUp.BLL.Common;
using GameTopUp.BLL.Exceptions;
using GameTopUp.DAL.Entities;
using Xunit;
using FluentAssertions;

namespace GameTopUp.Tests.UnitTests.Services
{
    public class OrderServiceTests
    {
        private readonly Mock<IOrderRepository> _orderRepoMock;
        private readonly Mock<IOrderHistoryRepository> _orderHistoryRepoMock;
        private readonly Mock<DatabaseContext> _dbMock;
        private readonly OrderService _orderService;

        public OrderServiceTests()
        {
            _orderRepoMock = new Mock<IOrderRepository>();
            _orderHistoryRepoMock = new Mock<IOrderHistoryRepository>();
            
            // Mocking DatabaseContext requires a DbConnection, but since we mock virtual methods, 
            // we can pass null or a dummy connection.
            _dbMock = new Mock<DatabaseContext>(Mock.Of<System.Data.Common.DbConnection>());

            // Setup the transaction wrapper to just execute the callback
            _dbMock.Setup(d => d.ExecuteInTransactionAsync(It.IsAny<Func<Task>>()))
                .Returns(async (Func<Task> action) => await action());

            _orderService = new OrderService(_orderRepoMock.Object, _orderHistoryRepoMock.Object, _dbMock.Object);
        }

        [Fact]
        public async Task PickOrderAsync_ShouldSucceed_WhenOrderIsPendingAndAdminPicks()
        {
            // Arrange
            long orderId = 123;
            var context = new UserContext(1, "admin_user", "Admin");
            
            _orderRepoMock.Setup(r => r.PickOrderAsync(orderId, context.UserId))
                .ReturnsAsync(1); // 1 row affected

            // Act
            await _orderService.PickOrderAsync(orderId, context);

            // Assert
            _orderRepoMock.Verify(r => r.PickOrderAsync(orderId, context.UserId), Times.Once);
            _orderHistoryRepoMock.Verify(r => r.CreateAsync(It.Is<OrderHistory>(h => 
                h.OrderId == orderId && 
                h.ToStatus == OrderStatus.Processing && 
                h.ActionBy == context.UserId &&
                h.IsAdmin == true)), Times.Once);
        }

        [Fact]
        public async Task PickOrderAsync_ShouldThrowBusinessException_WhenRaceConditionOccurs()
        {
            // Arrange
            long orderId = 123;
            var context = new UserContext(1, "admin_user", "Admin");
            
            _orderRepoMock.Setup(r => r.PickOrderAsync(orderId, context.UserId))
                .ReturnsAsync(0); // 0 rows affected (someone else picked it)
            
            _orderRepoMock.Setup(r => r.GetByIdAsync(orderId))
                .ReturnsAsync(new Order { Id = orderId, Status = OrderStatus.Processing });

            // Act
            Func<Task> act = () => _orderService.PickOrderAsync(orderId, context);

            // Assert
            await act.Should().ThrowAsync<BusinessException>()
                .WithMessage("Đơn hàng này đã được người khác tiếp nhận hoặc không còn ở trạng thái chờ.");
            
            _orderHistoryRepoMock.Verify(r => r.CreateAsync(It.IsAny<OrderHistory>()), Times.Never);
        }

        [Fact]
        public async Task PickOrderAsync_ShouldThrowNotFoundException_WhenOrderDoesNotExist()
        {
            // Arrange
            long orderId = 999;
            var context = new UserContext(1, "admin_user", "Admin");
            
            _orderRepoMock.Setup(r => r.PickOrderAsync(orderId, context.UserId))
                .ReturnsAsync(0);
            
            _orderRepoMock.Setup(r => r.GetByIdAsync(orderId))
                .ReturnsAsync((Order?)null);

            // Act
            Func<Task> act = () => _orderService.PickOrderAsync(orderId, context);

            // Assert
            await act.Should().ThrowAsync<NotFoundException>()
                .WithMessage($"Không tìm thấy đơn hàng #{orderId}");
        }

        [Fact]
        public async Task PickOrderAsync_ShouldThrowForbiddenException_WhenUserIsNotAdmin()
        {
            // Arrange
            long orderId = 123;
            var context = new UserContext(2, "member_user", "Member");

            // Act
            Func<Task> act = () => _orderService.PickOrderAsync(orderId, context);

            // Assert
            await act.Should().ThrowAsync<ForbiddenException>()
                .WithMessage("Chỉ Admin mới có quyền nhận xử lý đơn hàng.");
        }

        [Fact]
        public async Task CancelOrderAsync_ShouldReturnTrue_WhenOrderIsPending()
        {
            // Arrange
            long orderId = 123;
            var adminContext = new UserContext(1, "admin", "Admin");
            
            _orderRepoMock.Setup(r => r.CancelOrderAsync(orderId))
                .ReturnsAsync(1); // Success

            // Act
            var result = await _orderService.CancelOrderAsync(orderId, adminContext);

            // Assert
            result.Should().BeTrue();
            _orderRepoMock.Verify(r => r.CancelOrderAsync(orderId), Times.Once);
            _orderHistoryRepoMock.Verify(r => r.CreateAsync(It.Is<OrderHistory>(h => 
                h.OrderId == orderId && 
                h.ToStatus == OrderStatus.Cancelled && 
                h.ActionBy == adminContext.UserId)), Times.Once);
        }

        [Fact]
        public async Task CancelOrderAsync_ShouldReturnFalse_WhenOrderIsAlreadyProcessing()
        {
            // Arrange
            long orderId = 123;
            var adminContext = new UserContext(1, "admin", "Admin");
            
            _orderRepoMock.Setup(r => r.CancelOrderAsync(orderId))
                .ReturnsAsync(0); // Failed atomic update
            
            _orderRepoMock.Setup(r => r.GetByIdAsync(orderId))
                .ReturnsAsync(new Order { Id = orderId, Status = OrderStatus.Processing });

            // Act
            var result = await _orderService.CancelOrderAsync(orderId, adminContext);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task CompleteOrderAsync_ShouldSucceed_WhenValid()
        {
            // Arrange
            long orderId = 123;
            var context = new UserContext(1, "admin", "Admin");
            var order = new Order { Id = orderId, Status = OrderStatus.Processing, AssignTo = context.UserId };

            _orderRepoMock.Setup(r => r.GetByIdAsync(orderId)).ReturnsAsync(order);
            _orderRepoMock.Setup(r => r.CompleteOrderAsync(orderId, context.UserId)).ReturnsAsync(1);

            // Act
            await _orderService.CompleteOrderAsync(orderId, context);

            // Assert
            _orderRepoMock.Verify(r => r.CompleteOrderAsync(orderId, context.UserId), Times.Once);
            _orderHistoryRepoMock.Verify(r => r.CreateAsync(It.IsAny<OrderHistory>()), Times.Once);
        }

        [Fact]
        public async Task CompleteOrderAsync_ShouldBeIdempotent_WhenAlreadyCompleted()
        {
            // Arrange
            long orderId = 123;
            var context = new UserContext(1, "admin", "Admin");
            var order = new Order { Id = orderId, Status = OrderStatus.Completed };

            _orderRepoMock.Setup(r => r.GetByIdAsync(orderId)).ReturnsAsync(order);

            // Act
            await _orderService.CompleteOrderAsync(orderId, context);

            // Assert
            _orderRepoMock.Verify(r => r.CompleteOrderAsync(It.IsAny<long>(), It.IsAny<long>()), Times.Never);
            _orderHistoryRepoMock.Verify(r => r.CreateAsync(It.IsAny<OrderHistory>()), Times.Never);
        }

        [Fact]
        public async Task CompleteOrderAsync_ShouldThrowForbidden_WhenNotAdmin()
        {
            // Arrange
            var context = new UserContext(1, "user", "Member");

            // Act
            Func<Task> act = () => _orderService.CompleteOrderAsync(1, context);

            // Assert
            await act.Should().ThrowAsync<ForbiddenException>();
        }

        [Fact]
        public async Task CompleteOrderAsync_ShouldThrowBusiness_WhenNotProcessing()
        {
            // Arrange
            long orderId = 123;
            var context = new UserContext(1, "admin", "Admin");
            var order = new Order { Id = orderId, Status = OrderStatus.Pending };

            _orderRepoMock.Setup(r => r.GetByIdAsync(orderId)).ReturnsAsync(order);

            // Act
            Func<Task> act = () => _orderService.CompleteOrderAsync(orderId, context);

            // Assert
            await act.Should().ThrowAsync<BusinessException>()
                .WithMessage("*đang ở trạng thái Đang xử lý*");
        }

        [Fact]
        public async Task CompleteOrderAsync_ShouldThrowBusiness_WhenAdminMismatch()
        {
            // Arrange
            long orderId = 123;
            var context = new UserContext(1, "admin", "Admin");
            var order = new Order { Id = orderId, Status = OrderStatus.Processing, AssignTo = 2};
            
            _orderRepoMock.Setup(r => r.GetByIdAsync(orderId)).ReturnsAsync(order);
            
            // Act
            Func<Task> act = () => _orderService.CompleteOrderAsync(orderId, context);

            // Assert
            await act.Should().ThrowAsync<BusinessException>()
                .WithMessage("*đang được xử lý bởi một Admin khác.*");
        }

        [Fact]
        public async Task CompleteOrderAsync_ShouldHandleRaceCondition_ByCheckingFinalState()
        {
            // Arrange
            long orderId = 123;
            var context = new UserContext(1, "admin", "Admin");
            var order = new Order { Id = orderId, Status = OrderStatus.Processing, AssignTo = context.UserId };

            _orderRepoMock.Setup(r => r.GetByIdAsync(orderId)).ReturnsAsync(order);
            _orderRepoMock.Setup(r => r.CompleteOrderAsync(orderId, context.UserId)).ReturnsAsync(0); // Atomic update failed
            
            // Re-fetch returns Completed (meaning someone else finished it)
            _orderRepoMock.SetupSequence(r => r.GetByIdAsync(orderId))
                .ReturnsAsync(order)
                .ReturnsAsync(new Order { Id = orderId, Status = OrderStatus.Completed });

            // Act
            await _orderService.CompleteOrderAsync(orderId, context);

            // Assert: Should NOT throw exception because it's idempotent
            _orderHistoryRepoMock.Verify(r => r.CreateAsync(It.IsAny<OrderHistory>()), Times.Never);
        }
    }
}
