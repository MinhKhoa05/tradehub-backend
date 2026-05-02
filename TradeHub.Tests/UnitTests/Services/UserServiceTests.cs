using Moq;
using TradeHub.BLL.DTOs.Users;
using TradeHub.BLL.Exceptions;
using TradeHub.BLL.Services;
using TradeHub.DAL.Entities;
using TradeHub.DAL.Repositories.Interfaces;
using Xunit;
using FluentAssertions;
using TradeHub.BLL.Config;

namespace TradeHub.Tests.UnitTests.Services
{
    public class UserServiceTests
    {
        private readonly Mock<IUserRepository> _userRepoMock;
        private readonly UserService _userService;

        public UserServiceTests()
        {
            // Rationale: Mapster config is needed for DTO mapping validation
            MapsterConfig.RegisterMappings();
            
            _userRepoMock = new Mock<IUserRepository>();
            _userService = new UserService(_userRepoMock.Object);
        }

        #region --- Get Operations ---

        [Fact]
        public async Task GetAllAsync_ShouldReturnMappedDTOs_WithProperData()
        {
            // Arrange
            var users = new List<User>
            {
                new User { Id = 101, Username = "nguyenvana", Email = "vana@gmail.com", Role = UserRole.Member, Balance = 500000 },
                new User { Id = 102, Username = "tranvanb", Email = "vanb@yahoo.com", Role = UserRole.Admin, Balance = 0 }
            };
            _userRepoMock.Setup(r => r.GetAllAsync(1, 10)).ReturnsAsync(users);

            // Act
            var result = await _userService.GetAllAsync(1, 10);

            // Assert
            result.Should().HaveCount(2);
            var first = result.First();
            first.Id.Should().Be(101);
            first.Username.Should().Be("nguyenvana");
            first.Email.Should().Be("vana@gmail.com");
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnDTO_WhenUserExists()
        {
            // Arrange
            var user = new User { Id = 55, Username = "testuser", Email = "test@tradehub.vn", Balance = 1000 };
            _userRepoMock.Setup(r => r.GetByIdAsync(55)).ReturnsAsync(user);

            // Act
            var result = await _userService.GetByIdAsync(55);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(55);
            result.Username.Should().Be("testuser");
            result.Email.Should().Be("test@tradehub.vn");
        }

        [Fact]
        public async Task GetByIdAsync_ShouldThrowNotFound_WhenUserDoesNotExist()
        {
            // Arrange
            _userRepoMock.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((User?)null);

            // Act
            Func<Task> act = () => _userService.GetByIdAsync(999);

            // Assert
            await act.Should().ThrowAsync<NotFoundException>()
                .WithMessage("Người dùng không tồn tại.");
        }

        #endregion

        #region --- Write Operations ---

        [Fact]
        public async Task RegisterAsync_ShouldCreateUser_WhenEmailIsUnique()
        {
            // Arrange
            var request = new CreateUserRequest { Name = "New User", Email = "new@test.com", Password = "hashed_password" };
            _userRepoMock.Setup(r => r.GetByEmailAsync(request.Email)).ReturnsAsync((User?)null);
            _userRepoMock.Setup(r => r.CreateAsync(It.IsAny<User>())).ReturnsAsync(1);

            // Act
            var userId = await _userService.RegisterAsync(request);

            // Assert
            userId.Should().Be(1);
            _userRepoMock.Verify(r => r.CreateAsync(It.Is<User>(u => 
                u.Username == request.Name && 
                u.Email == request.Email && 
                u.PasswordHash == request.Password)), Times.Once);
        }

        [Fact]
        public async Task RegisterAsync_ShouldThrow_WhenEmailAlreadyExists()
        {
            // Arrange
            var request = new CreateUserRequest { Name = "New User", Email = "existing@test.com" };
            _userRepoMock.Setup(r => r.GetByEmailAsync(request.Email)).ReturnsAsync(new User());

            // Act
            Func<Task> act = () => _userService.RegisterAsync(request);

            // Assert
            await act.Should().ThrowAsync<BusinessException>().WithMessage("Email này đã được sử dụng trong hệ thống.");
            _userRepoMock.Verify(r => r.CreateAsync(It.IsAny<User>()), Times.Never);
        }

        [Fact]
        public async Task UpdateProfileAsync_ShouldCorrectlyMapAndSave_WhenUserExists()
        {
            // Arrange
            var existingUser = new User { Id = 1, Username = "old_name", Email = "old@test.com", IsActive = true };
            var request = new UpdateUserRequest { Username = "new_name", IsActive = false };
            _userRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(existingUser);

            // Act
            await _userService.UpdateProfileAsync(1, request);

            // Assert
            existingUser.Username.Should().Be("new_name");
            existingUser.Email.Should().Be("old@test.com"); // Email should stay the same
            existingUser.IsActive.Should().BeFalse(); // Soft status update
            _userRepoMock.Verify(r => r.UpdateAsync(existingUser), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_ShouldPerformSoftDelete_ByCallingRepo()
        {
            // Arrange
            var user = new User { Id = 77, IsActive = true };
            _userRepoMock.Setup(r => r.GetByIdAsync(77)).ReturnsAsync(user);

            // Act
            await _userService.DeleteAsync(77);

            // Assert
            // Rationale: We verify that the service checks for existence before calling delete
            _userRepoMock.Verify(r => r.GetByIdAsync(77), Times.Once);
            _userRepoMock.Verify(r => r.DeleteAsync(77), Times.Once);
        }

        #endregion
    }
}
