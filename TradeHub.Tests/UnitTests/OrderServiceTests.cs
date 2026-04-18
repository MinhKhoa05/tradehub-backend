using FluentAssertions;
using Moq;
using TradeHub.BLL.Common;
using TradeHub.BLL.DTOs.Orders;
using TradeHub.BLL.Exceptions;
using TradeHub.BLL.Services;
using TradeHub.DAL;
using TradeHub.DAL.Entities;
using TradeHub.DAL.Repositories.Interfaces;
using Xunit;

namespace TradeHub.Tests.UnitTests
{
    public class OrderServiceTests
    {
        private readonly Mock<IOrderRepository> _orderRepoMock;
        private readonly Mock<IOrderItemRepository> _orderItemRepoMock;
        private readonly Mock<IOrderHistoryRepository> _orderHistoryRepoMock;
        private readonly Mock<DatabaseContext> _databaseMock;
        private readonly Mock<IIdentityService> _identityServiceMock;
        private readonly OrderService _orderService;

        public OrderServiceTests()
        {
            _orderRepoMock = new Mock<IOrderRepository>();
            _orderItemRepoMock = new Mock<IOrderItemRepository>();
            _orderHistoryRepoMock = new Mock<IOrderHistoryRepository>();
            
            // DatabaseContext requires connection string in constructor
            _databaseMock = new Mock<DatabaseContext>("Server=fake");
            
            _identityServiceMock = new Mock<IIdentityService>();
            
            // Setup default user
            _identityServiceMock.Setup(x => x.UserId).Returns(1);

            _orderService = new OrderService(
                _orderRepoMock.Object,
                _orderItemRepoMock.Object,
                _orderHistoryRepoMock.Object,
                _databaseMock.Object,
                _identityServiceMock.Object);
        }

        [Fact]
        public async Task UpdateStatusAsync_WhenOrderDoesNotExist_ShouldThrowBusinessException()
        {
            // Arrange
            int orderId = 999;
            _orderRepoMock.Setup(x => x.GetByIdAsync(orderId)).ReturnsAsync((Order?)null);

            // Act
            Func<Task> act = async () => await _orderService.UpdateStatusAsync(orderId, new UpdateOrderStatusRequest());

            // Assert
            await act.Should().ThrowAsync<BusinessException>()
                .WithMessage("Đơn hàng không tồn tại.");
        }

        [Fact]
        public async Task UpdateStatusAsync_WhenOrderDoesNotBelongToUser_ShouldThrowBusinessException()
        {
            // Arrange
            int orderId = 1;
            var order = new Order { Id = orderId };
            _orderRepoMock.Setup(x => x.GetByIdAsync(orderId)).ReturnsAsync(order);
            _orderRepoMock.Setup(x => x.IsOrderBelongsToUserAsync(1, orderId)).ReturnsAsync(false);

            // Act
            Func<Task> act = async () => await _orderService.UpdateStatusAsync(orderId, new UpdateOrderStatusRequest());

            // Assert
            await act.Should().ThrowAsync<BusinessException>()
                .WithMessage("Bạn không có quyền truy cập vào đơn hàng này.");
        }

        [Fact]
        public async Task UpdateStatusAsync_WhenInvalidStatusTransition_ShouldThrowBusinessException()
        {
            // Arrange
            int orderId = 1;
            var order = new Order { Id = orderId, Status = OrderStatus.Delivered }; // Delivered cannot go to Confirmed
            _orderRepoMock.Setup(x => x.GetByIdAsync(orderId)).ReturnsAsync(order);
            _orderRepoMock.Setup(x => x.IsOrderBelongsToUserAsync(1, orderId)).ReturnsAsync(true);
            
            var request = new UpdateOrderStatusRequest { ToStatus = OrderStatus.Confirmed };

            // Act
            Func<Task> act = async () => await _orderService.UpdateStatusAsync(orderId, request);

            // Assert
            await act.Should().ThrowAsync<BusinessException>()
                .WithMessage("*Không thể chuyển trạng thái*");
        }

        [Fact]
        public async Task UpdateStatusAsync_WhenValid_ShouldCallUpdateAndCreateHistory()
        {
            // Arrange
            int orderId = 1;
            var order = new Order { Id = orderId, Status = OrderStatus.Pending };
            _orderRepoMock.Setup(x => x.GetByIdAsync(orderId)).ReturnsAsync(order);
            _orderRepoMock.Setup(x => x.IsOrderBelongsToUserAsync(1, orderId)).ReturnsAsync(true);
            _orderRepoMock.Setup(x => x.UpdateStatusAsync(orderId, OrderStatus.Confirmed)).ReturnsAsync(1);

            // Setup transaction mock to execute the callback
            _databaseMock.Setup(x => x.ExecuteInTransactionAsync(It.IsAny<Func<Task>>()))
                .Callback<Func<Task>>(async action => await action())
                .Returns(Task.CompletedTask);

            var request = new UpdateOrderStatusRequest 
            { 
                ToStatus = OrderStatus.Confirmed,
                ActorType = ActorType.Seller,
                Note = "Confirmed by seller"
            };

            // Act
            await _orderService.UpdateStatusAsync(orderId, request);

            // Assert
            _orderRepoMock.Verify(x => x.UpdateStatusAsync(orderId, OrderStatus.Confirmed), Times.Once);
            _orderHistoryRepoMock.Verify(x => x.CreateAsync(It.Is<OrderHistory>(h => 
                h.OrderId == orderId && 
                h.FromStatus == OrderStatus.Pending && 
                h.ToStatus == OrderStatus.Confirmed &&
                h.Note == "Confirmed by seller")), Times.Once);
        }
    }
}
