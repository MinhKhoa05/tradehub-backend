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
        public async Task PickOrderAsync_ShouldSucceed_WhenOrderIsPaid()
        {
            // Arrange
            var admin = new UserContext(1, "admin", "Admin");
            var order = new Order { Id = 123, Status = OrderStatus.Paid, AssignTo = 0 };

            // Act
            await _orderService.PickOrderAsync(order, admin);

            // Assert
            order.Status.Should().Be(OrderStatus.Processing);
            order.AssignTo.Should().Be(admin.UserId);
            
            _orderRepoMock.Verify(r => r.UpdateAsync(It.Is<Order>(o => o.Id == 123 && o.Status == OrderStatus.Processing)), Times.Once);
            _orderHistoryRepoMock.Verify(r => r.CreateAsync(It.Is<OrderHistory>(h => h.ToStatus == OrderStatus.Processing)), Times.Once);
        }

        [Fact]
        public async Task PickOrderAsync_ShouldThrowBusinessException_WhenOrderNotPaid()
        {
            // Arrange
            var admin = new UserContext(1, "admin", "Admin");
            var order = new Order { Status = OrderStatus.Pending };

            // Act
            Func<Task> act = () => _orderService.PickOrderAsync(order, admin);

            // Assert
            await act.Should().ThrowAsync<BusinessException>()
                .WithMessage("Chỉ có thể tiếp nhận đơn hàng đã thanh toán.");
        }

        [Fact]
        public async Task PickOrderAsync_ShouldThrowBusinessException_WhenOrderAlreadyAssigned()
        {
            // Arrange
            var admin = new UserContext(1, "admin", "Admin");
            var order = new Order { Status = OrderStatus.Processing, AssignTo = 2 };

            // Act
            Func<Task> act = () => _orderService.PickOrderAsync(order, admin);

            // Assert
            await act.Should().ThrowAsync<BusinessException>()
                .WithMessage("Đơn hàng đã được admin khác tiếp nhận.");
        }

        [Fact]
        public async Task CompleteOrderAsync_ShouldSucceed_WhenValid()
        {
            // Arrange
            var admin = new UserContext(1, "admin", "Admin");
            var order = new Order { Id = 123, Status = OrderStatus.Processing, AssignTo = admin.UserId };

            // Act
            await _orderService.CompleteOrderAsync(order, admin);

            // Assert
            order.Status.Should().Be(OrderStatus.Completed);
            
            _orderRepoMock.Verify(r => r.UpdateAsync(It.Is<Order>(o => o.Id == 123 && o.Status == OrderStatus.Completed)), Times.Once);
            _orderHistoryRepoMock.Verify(r => r.CreateAsync(It.Is<OrderHistory>(h => h.ToStatus == OrderStatus.Completed)), Times.Once);
        }

        [Fact]
        public async Task CompleteOrderAsync_ShouldThrowBusinessException_WhenOrderNotProcessing()
        {
            // Arrange
            var admin = new UserContext(1, "admin", "Admin");
            var order = new Order { Status = OrderStatus.Paid, AssignTo = admin.UserId };

            // Act
            Func<Task> act = () => _orderService.CompleteOrderAsync(order, admin);

            // Assert
            await act.Should().ThrowAsync<BusinessException>()
                .WithMessage("Trạng thái đơn hàng không hợp lệ để hoàn thành.");
        }

        [Fact]
        public async Task CompleteOrderAsync_ShouldThrowBusinessException_WhenAssignedToAnotherAdmin()
        {
            // Arrange
            var admin = new UserContext(1, "admin", "Admin");
            var order = new Order { Status = OrderStatus.Processing, AssignTo = 999 };

            // Act
            Func<Task> act = () => _orderService.CompleteOrderAsync(order, admin);

            // Assert
            await act.Should().ThrowAsync<BusinessException>()
                .WithMessage("Bạn không thể can thiệp vào đơn hàng của người khác.");
        }

        [Fact]
        public async Task CancelOrderAsync_ShouldSucceed_WhenValid()
        {
            // Arrange
            var admin = new UserContext(1, "admin", "Admin");
            var order = new Order { Id = 123, Status = OrderStatus.Pending, UserId = 999, AssignTo = admin.UserId };

            // Act
            var result = await _orderService.CancelOrderAsync(order, admin);

            // Assert
            result.Should().Be(OrderStatus.Pending);
            order.Status.Should().Be(OrderStatus.Cancelled);
            
            _orderRepoMock.Verify(r => r.UpdateAsync(It.Is<Order>(o => o.Id == 123 && o.Status == OrderStatus.Cancelled)), Times.Once);
            _orderHistoryRepoMock.Verify(r => r.CreateAsync(It.Is<OrderHistory>(h => h.ToStatus == OrderStatus.Cancelled)), Times.Once);
        }

        [Fact]
        public async Task CancelOrderAsync_ShouldBeIdempotent_WhenAlreadyCancelled()
        {
            // Arrange
            var admin = new UserContext(1, "admin", "Admin");
            var order = new Order { Status = OrderStatus.Cancelled };

            // Act
            var result = await _orderService.CancelOrderAsync(order, admin);

            // Assert
            result.Should().BeNull();
            _orderRepoMock.Verify(r => r.UpdateAsync(It.IsAny<Order>()), Times.Never);
            _orderHistoryRepoMock.Verify(r => r.CreateAsync(It.IsAny<OrderHistory>()), Times.Never);
        }

        [Fact]
        public async Task CancelOrderAsync_ShouldThrowBusinessException_WhenOrderCompleted()
        {
            // Arrange
            var admin = new UserContext(1, "admin", "Admin");
            var order = new Order { Status = OrderStatus.Completed };

            // Act
            Func<Task> act = () => _orderService.CancelOrderAsync(order, admin);

            // Assert
            await act.Should().ThrowAsync<BusinessException>()
                .WithMessage("Đơn hàng đã hoàn thành không thể hủy.");
        }
    }
}
