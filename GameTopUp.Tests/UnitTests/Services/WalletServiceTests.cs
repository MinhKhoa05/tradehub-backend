using Moq;
using GameTopUp.BLL.Services;
using GameTopUp.DAL.Interfaces;
using GameTopUp.DAL.Entities;
using GameTopUp.BLL.Exceptions;
using Xunit;
using FluentAssertions;

namespace GameTopUp.Tests.UnitTests.Services
{
    public class WalletServiceTests
    {
        private readonly Mock<IWalletRepository> _walletRepoMock;
        private readonly Mock<IWalletTransactionRepository> _walletTxRepoMock;
        private readonly WalletService _walletService;

        public WalletServiceTests()
        {
            _walletRepoMock = new Mock<IWalletRepository>();
            _walletTxRepoMock = new Mock<IWalletTransactionRepository>();
            _walletService = new WalletService(_walletRepoMock.Object, _walletTxRepoMock.Object);
        }

        [Fact]
        public async Task CreditAsync_ShouldUpdateBalanceAndRecordHistory()
        {
            // Arrange
            var wallet = new Wallet { UserId = 1, Balance = 100 };
            decimal amount = 50;
            _walletRepoMock.Setup(r => r.IncreaseBalanceAsync(1, amount)).ReturnsAsync(1);
            _walletTxRepoMock.Setup(r => r.CreateAsync(It.IsAny<WalletTransaction>())).ReturnsAsync(999);

            // Act
            var result = await _walletService.CreditAsync(wallet, amount, WalletTransactionType.Deposit, "Test deposit");

            // Assert
            wallet.Balance.Should().Be(150);
            _walletRepoMock.Verify(r => r.IncreaseBalanceAsync(1, amount), Times.Once);
            _walletTxRepoMock.Verify(r => r.CreateAsync(It.Is<WalletTransaction>(tx => 
                tx.Amount == amount && 
                tx.BalanceBefore == 100 && 
                tx.BalanceAfter == 150)), Times.Once);
        }

        [Fact]
        public async Task DebitAsync_ShouldUpdateBalanceAndRecordHistory()
        {
            // Arrange
            var wallet = new Wallet { UserId = 1, Balance = 200 };
            decimal amount = 80;
            _walletRepoMock.Setup(r => r.DecreaseBalanceAsync(1, amount)).ReturnsAsync(1);
            _walletTxRepoMock.Setup(r => r.CreateAsync(It.IsAny<WalletTransaction>())).ReturnsAsync(999);

            // Act
            var result = await _walletService.DebitAsync(wallet, amount, WalletTransactionType.Withdraw, "Test withdraw");

            // Assert
            wallet.Balance.Should().Be(120);
            _walletRepoMock.Verify(r => r.DecreaseBalanceAsync(1, amount), Times.Once);
            _walletTxRepoMock.Verify(r => r.CreateAsync(It.Is<WalletTransaction>(tx => 
                tx.Amount == -amount && 
                tx.BalanceBefore == 200 && 
                tx.BalanceAfter == 120)), Times.Once);
        }

        [Fact]
        public async Task DebitAsync_ShouldThrow_WhenBalanceInsufficient()
        {
            // Arrange
            var wallet = new Wallet { UserId = 1, Balance = 50 };
            decimal amount = 100;

            // Act
            Func<Task> act = () => _walletService.DebitAsync(wallet, amount, WalletTransactionType.Withdraw, "Test");

            // Assert
            await act.Should().ThrowAsync<BusinessException>().WithMessage("Số dư ví không đủ*");
        }
    }
}
