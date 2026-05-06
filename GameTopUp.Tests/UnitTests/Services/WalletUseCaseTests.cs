using Moq;
using GameTopUp.BLL.ApplicationServices;
using GameTopUp.BLL.Services;
using GameTopUp.BLL.Common;
using GameTopUp.DAL.Interfaces;
using GameTopUp.DAL.Entities;
using GameTopUp.DAL;
using Xunit;
using FluentAssertions;
using GameTopUp.BLL.DTOs.Wallets;

namespace GameTopUp.Tests.UnitTests.Services
{
    public class WalletUseCaseTests
    {
        private readonly Mock<IWalletRepository> _walletRepoMock;
        private readonly Mock<IWalletTransactionRepository> _walletTxRepoMock;
        private readonly Mock<DatabaseContext> _dbMock;
        private readonly WalletService _walletService;
        private readonly WalletUseCase _walletUseCase;

        public WalletUseCaseTests()
        {
            _walletRepoMock = new Mock<IWalletRepository>();
            _walletTxRepoMock = new Mock<IWalletTransactionRepository>();
            
            _dbMock = new Mock<DatabaseContext>(Mock.Of<System.Data.Common.DbConnection>());
            _dbMock.Setup(d => d.ExecuteInTransactionAsync(It.IsAny<Func<Task<TransactionResponseDTO>>>()))
                .Returns(async (Func<Task<TransactionResponseDTO>> action) => await action());

            _walletService = new WalletService(_walletRepoMock.Object, _walletTxRepoMock.Object);
            _walletUseCase = new WalletUseCase(_walletService, _dbMock.Object);
        }

        [Fact]
        public async Task DepositAsync_ShouldLockWalletAndCredit()
        {
            // Arrange
            var context = new UserContext(1, "user", "Customer");
            var wallet = new Wallet { UserId = 1, Balance = 100 };
            
            _walletRepoMock.Setup(r => r.GetByUserIdForUpdateAsync(1)).ReturnsAsync(wallet);
            _walletRepoMock.Setup(r => r.IncreaseBalanceAsync(1, 50)).ReturnsAsync(1);
            _walletTxRepoMock.Setup(r => r.CreateAsync(It.IsAny<WalletTransaction>())).ReturnsAsync(999);

            // Act
            await _walletUseCase.DepositAsync(context, 50);

            // Assert
            _walletRepoMock.Verify(r => r.GetByUserIdForUpdateAsync(1), Times.Once);
            _walletRepoMock.Verify(r => r.IncreaseBalanceAsync(1, 50), Times.Once);
        }

        [Fact]
        public async Task WithdrawAsync_ShouldLockWalletAndDebit()
        {
            // Arrange
            var context = new UserContext(1, "user", "Customer");
            var wallet = new Wallet { UserId = 1, Balance = 100 };
            
            _walletRepoMock.Setup(r => r.GetByUserIdForUpdateAsync(1)).ReturnsAsync(wallet);
            _walletRepoMock.Setup(r => r.DecreaseBalanceAsync(1, 40)).ReturnsAsync(1);
            _walletTxRepoMock.Setup(r => r.CreateAsync(It.IsAny<WalletTransaction>())).ReturnsAsync(999);

            // Act
            await _walletUseCase.WithdrawAsync(context, 40);

            // Assert
            _walletRepoMock.Verify(r => r.GetByUserIdForUpdateAsync(1), Times.Once);
            _walletRepoMock.Verify(r => r.DecreaseBalanceAsync(1, 40), Times.Once);
        }
    }
}
