using Moq;
using GameTopUp.BLL.Services;
using GameTopUp.DAL.Interfaces;
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
        private readonly OrderService _orderService;

        public OrderServiceTests()
        {
            _orderRepoMock = new Mock<IOrderRepository>();
            _orderHistoryRepoMock = new Mock<IOrderHistoryRepository>();
            _orderService = new OrderService(_orderRepoMock.Object, _orderHistoryRepoMock.Object);
        }

        [Fact]
        public async Task PickOrder_ShouldSucceed_WhenOrderIsPending()
        {
            // Arrange
            var admin = new UserContext(1, "admin", "Admin");
            var order = new Order { Id = 123, Status = OrderStatus.Pending, AssignTo = 0 };

            // Act
            await _orderService.PickOrder(order, admin);

            // Assert
            order.Status.Should().Be(OrderStatus.Processing);
            order.AssignTo.Should().Be(admin.UserId);
            
            _orderRepoMock.Verify(r => r.UpdateAsync(It.Is<Order>(o => o.Id == 123 && o.Status == OrderStatus.Processing)), Times.Once);
            _orderHistoryRepoMock.Verify(r => r.CreateAsync(It.Is<OrderHistory>(h => h.ToStatus == OrderStatus.Processing)), Times.Once);
        }

        [Fact]
        public async Task PickOrder_ShouldThrowBusinessException_WhenOrderNotPending()
        {
            // Arrange
            var admin = new UserContext(1, "admin", "Admin");
            var order = new Order { Status = OrderStatus.Cancelled };

            // Act
            Func<Task> act = () => _orderService.PickOrder(order, admin);

            // Assert
            await act.Should().ThrowAsync<BusinessException>()
                .WithMessage("Đơn hàng không còn ở trạng thái chờ.");
        }

        [Fact]
        public async Task PickOrder_ShouldThrowBusinessException_WhenOrderAlreadyAssigned()
        {
            // Arrange
            var admin = new UserContext(1, "admin", "Admin");
            var order = new Order { Status = OrderStatus.Pending, AssignTo = 2 };

            // Act
            Func<Task> act = () => _orderService.PickOrder(order, admin);

            // Assert
            await act.Should().ThrowAsync<BusinessException>()
                .WithMessage("Đơn hàng đã được tiếp nhận bởi người khác.");
        }

        [Fact]
        public async Task CompleteOrder_ShouldSucceed_WhenValid()
        {
            // Arrange
            var admin = new UserContext(1, "admin", "Admin");
            var order = new Order { Id = 123, Status = OrderStatus.Processing, AssignTo = admin.UserId };

            // Act
            await _orderService.CompleteOrder(order, admin);

            // Assert
            order.Status.Should().Be(OrderStatus.Completed);
            
            _orderRepoMock.Verify(r => r.UpdateAsync(It.Is<Order>(o => o.Id == 123 && o.Status == OrderStatus.Completed)), Times.Once);
            _orderHistoryRepoMock.Verify(r => r.CreateAsync(It.Is<OrderHistory>(h => h.ToStatus == OrderStatus.Completed)), Times.Once);
        }

        [Fact]
        public async Task CompleteOrder_ShouldThrowBusinessException_WhenOrderNotProcessing()
        {
            // Arrange
            var admin = new UserContext(1, "admin", "Admin");
            var order = new Order { Status = OrderStatus.Pending, AssignTo = admin.UserId };

            // Act
            Func<Task> act = () => _orderService.CompleteOrder(order, admin);

            // Assert
            await act.Should().ThrowAsync<BusinessException>()
                .WithMessage("Trạng thái đơn hàng không hợp lệ để hoàn thành.");
        }

        [Fact]
        public async Task CompleteOrder_ShouldThrowBusinessException_WhenAssignedToAnotherAdmin()
        {
            // Arrange
            var admin = new UserContext(1, "admin", "Admin");
            var order = new Order { Status = OrderStatus.Processing, AssignTo = 999 };

            // Act
            Func<Task> act = () => _orderService.CompleteOrder(order, admin);

            // Assert
            await act.Should().ThrowAsync<BusinessException>()
                .WithMessage("Bạn không thể can thiệp vào đơn hàng của người khác.");
        }

        [Fact]
        public async Task CancelOrder_ShouldSucceed_WhenValid()
        {
            // Arrange
            var admin = new UserContext(1, "admin", "Admin");
            var order = new Order { Id = 123, Status = OrderStatus.Pending };

            // Act
            var result = await _orderService.CancelOrder(order, admin);

            // Assert
            result.Should().BeTrue();
            order.Status.Should().Be(OrderStatus.Cancelled);
            
            _orderRepoMock.Verify(r => r.UpdateAsync(It.Is<Order>(o => o.Id == 123 && o.Status == OrderStatus.Cancelled)), Times.Once);
            _orderHistoryRepoMock.Verify(r => r.CreateAsync(It.Is<OrderHistory>(h => h.ToStatus == OrderStatus.Cancelled)), Times.Once);
        }

        [Fact]
        public async Task CancelOrder_ShouldBeIdempotent_WhenAlreadyCancelled()
        {
            // Arrange
            var admin = new UserContext(1, "admin", "Admin");
            var order = new Order { Status = OrderStatus.Cancelled };

            // Act
            var result = await _orderService.CancelOrder(order, admin);

            // Assert
            result.Should().BeFalse();
            _orderRepoMock.Verify(r => r.UpdateAsync(It.IsAny<Order>()), Times.Never);
            _orderHistoryRepoMock.Verify(r => r.CreateAsync(It.IsAny<OrderHistory>()), Times.Never);
        }

        [Fact]
        public async Task CancelOrder_ShouldThrowBusinessException_WhenOrderNotPending()
        {
            // Arrange
            var admin = new UserContext(1, "admin", "Admin");
            var order = new Order { Status = OrderStatus.Processing };

            // Act
            Func<Task> act = () => _orderService.CancelOrder(order, admin);

            // Assert
            await act.Should().ThrowAsync<BusinessException>()
                .WithMessage("Không thể hủy đơn hàng đang ở trạng thái này.");
        }
    }
}
