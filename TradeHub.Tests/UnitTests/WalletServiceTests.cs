using FluentAssertions;
using Moq;
using TradeHub.BLL.Common;
using TradeHub.BLL.Exceptions;
using TradeHub.BLL.Services;
using TradeHub.DAL;
using TradeHub.DAL.Entities;
using TradeHub.DAL.Repositories.Interfaces;
using Xunit;

namespace TradeHub.Tests.UnitTests
{
    public class WalletServiceTests
    {
        private readonly Mock<IWalletRepository> _walletRepoMock;
        private readonly Mock<IWalletTransactionRepository> _walletTxRepoMock;
        private readonly Mock<DatabaseContext> _databaseMock;
        private readonly Mock<IIdentityService> _identityServiceMock;
        private readonly WalletService _walletService;

        private const long CurrentUserId = 1;

        public WalletServiceTests()
        {
            _walletRepoMock = new Mock<IWalletRepository>();
            _walletTxRepoMock = new Mock<IWalletTransactionRepository>();
            _databaseMock = new Mock<DatabaseContext>("Server=fake");
            _identityServiceMock = new Mock<IIdentityService>();

            _identityServiceMock.Setup(x => x.UserId).Returns(CurrentUserId);

            // Default Setup: Auto-execute transaction callback and rethrow
            _databaseMock.Setup(x => x.ExecuteInTransactionAsync(It.IsAny<Func<Task>>()))
                .Returns<Func<Task>>(async action => await action());

            _walletService = new WalletService(
                _walletRepoMock.Object,
                _walletTxRepoMock.Object,
                _databaseMock.Object,
                _identityServiceMock.Object);
        }

        [Fact]
        public async Task DepositAsync_WhenAmountIsValid_ShouldIncreaseBalanceAndReturnTx()
        {
            // Arrange
            int amount = 100000;
            var wallet = new Wallet { Id = 1, UserId = CurrentUserId, Balance = 50000 };
            _walletRepoMock.Setup(x => x.GetByUserIdAsync(CurrentUserId)).ReturnsAsync(wallet);
            _walletRepoMock.Setup(x => x.IncreaseBalanceAsync(CurrentUserId, amount)).ReturnsAsync(1);
            _walletTxRepoMock.Setup(x => x.CreateAsync(It.IsAny<WalletTransaction>())).ReturnsAsync(100);

            // Act
            var result = await _walletService.DepositAsync(amount);

            // Assert
            result.Amount.Should().Be(amount);
            result.Type.Should().Be(WalletTransactionType.Deposit);
            _walletRepoMock.Verify(x => x.IncreaseBalanceAsync(CurrentUserId, amount), Times.Once);
            _walletTxRepoMock.Verify(x => x.CreateAsync(It.IsAny<WalletTransaction>()), Times.Once);
        }

        [Fact]
        public async Task WithdrawAsync_WhenAmountGreaterThanBalance_ShouldThrowBusinessException()
        {
            // Arrange
            int amount = 150000;
            var wallet = new Wallet { Id = 1, UserId = CurrentUserId, Balance = 100000 };
            _walletRepoMock.Setup(x => x.GetByUserIdAsync(CurrentUserId)).ReturnsAsync(wallet);
            
            // Theo logic trong code, affected = 0 nghĩa là balance < amount (do điều kiện query)
            _walletRepoMock.Setup(x => x.DecreaseBalanceAsync(CurrentUserId, amount)).ReturnsAsync(0);

            // Act
            Func<Task> act = async () => await _walletService.WithdrawAsync(amount);

            // Assert
            await act.Should().ThrowAsync<BusinessException>().WithMessage("*Số dư không đủ*");
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-5000)]
        public async Task ProcessTransaction_WhenAmountIsZeroOrNegative_ShouldThrowBusinessException(int amount)
        {
            // Act
            Func<Task> act1 = async () => await _walletService.DepositAsync(amount);
            Func<Task> act2 = async () => await _walletService.WithdrawAsync(amount);

            // Assert
            await act1.Should().ThrowAsync<BusinessException>().WithMessage("*phải lớn hơn 0*");
            await act2.Should().ThrowAsync<BusinessException>().WithMessage("*phải lớn hơn 0*");
        }

        [Fact]
        public async Task WithdrawAsync_ConcurrencyTest_ShouldExecuteSequentiallyViaMockCount()
        {
            // Arrange
            int withdrawCount = 10;
            int amountPerWithdraw = 10000;
            var wallet = new Wallet { Id = 1, UserId = CurrentUserId, Balance = 1000000 }; // Dư sức rút
            
            _walletRepoMock.Setup(x => x.GetByUserIdAsync(CurrentUserId)).ReturnsAsync(wallet);
            _walletRepoMock.Setup(x => x.DecreaseBalanceAsync(CurrentUserId, amountPerWithdraw)).ReturnsAsync(1);

            // Act
            var tasks = new List<Task>();
            for (int i = 0; i < withdrawCount; i++)
            {
                tasks.Add(_walletService.WithdrawAsync(amountPerWithdraw));
            }
            await Task.WhenAll(tasks);

            // Assert
            // Kiểm tra xem DecreaseBalanceAsync được gọi đúng số lần không
            _walletRepoMock.Verify(x => x.DecreaseBalanceAsync(CurrentUserId, amountPerWithdraw), Times.Exactly(withdrawCount));
            _walletTxRepoMock.Verify(x => x.CreateAsync(It.IsAny<WalletTransaction>()), Times.Exactly(withdrawCount));
        }
    }
}
