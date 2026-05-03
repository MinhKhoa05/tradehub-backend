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
        public async Task CancelOrderAsync_ShouldSucceed_WhenOrderIsPending()
        {
            // Arrange
            long orderId = 123;
            var adminContext = new UserContext(1, "admin", "Admin");
            
            _orderRepoMock.Setup(r => r.CancelOrderAsync(orderId))
                .ReturnsAsync(1); // Success

            // Act
            await _orderService.CancelOrderAsync(orderId, adminContext);

            // Assert
            _orderRepoMock.Verify(r => r.CancelOrderAsync(orderId), Times.Once);
            _orderHistoryRepoMock.Verify(r => r.CreateAsync(It.Is<OrderHistory>(h => 
                h.OrderId == orderId && 
                h.ToStatus == OrderStatus.Cancelled && 
                h.ActionBy == adminContext.UserId)), Times.Once);
        }

        [Fact]
        public async Task CancelOrderAsync_ShouldThrowBusinessException_WhenOrderIsAlreadyProcessing()
        {
            // Arrange
            long orderId = 123;
            var adminContext = new UserContext(1, "admin", "Admin");
            
            _orderRepoMock.Setup(r => r.CancelOrderAsync(orderId))
                .ReturnsAsync(0); // Failed atomic update
            
            _orderRepoMock.Setup(r => r.GetByIdAsync(orderId))
                .ReturnsAsync(new Order { Id = orderId, Status = OrderStatus.Processing });

            // Act
            Func<Task> act = () => _orderService.CancelOrderAsync(orderId, adminContext);

            // Assert
            await act.Should().ThrowAsync<BusinessException>()
                .WithMessage($"Không thể hủy đơn hàng. Trạng thái hiện tại: {OrderStatus.Processing}");
        }
    }
}
