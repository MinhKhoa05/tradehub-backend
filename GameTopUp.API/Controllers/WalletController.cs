using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GameTopUp.BLL.Services;
using GameTopUp.BLL.ApplicationServices;
using GameTopUp.BLL.DTOs.Wallets;

namespace GameTopUp.API.Controllers
{
    [Authorize]
    [Route("api/wallet")]
    [ApiController]
    public class WalletController : ApiControllerBase
    {
        private readonly WalletService _walletService;
        private readonly WalletUseCase _walletUseCase;

        public WalletController(WalletService walletService, WalletUseCase walletUseCase)
        {
            _walletService = walletService;
            _walletUseCase = walletUseCase;
        }

        [HttpPost("active")]
        public async Task<IActionResult> ActiveWallet()
        {
            var walletId = await _walletUseCase.ActiveWalletAsync(CurrentUser);
            return ApiOk(new { WalletId = walletId }, "Kích hoạt ví thành công.");
        }

        [HttpGet]
        public async Task<IActionResult> GetBalance()
        {
            var balance = await _walletService.GetBalanceAsync(CurrentUser);
            return ApiOk(balance);
        }

        [HttpGet("transactions")]
        public async Task<IActionResult> GetWalletTransactions()
        {
            var transactions = await _walletService.GetTransactionsAsync(CurrentUser);
            return ApiOk(transactions);
        }

        [HttpPost("transactions/deposit")]
        public async Task<IActionResult> Deposit([FromBody] WalletTransactionRequest request)
        {
            var response = await _walletUseCase.DepositAsync(CurrentUser, request.Amount);
            return ApiOk(response, "Nạp tiền thành công.");
        }

        [HttpPost("transactions/withdraw")]
        public async Task<IActionResult> Withdraw([FromBody] WalletTransactionRequest request)
        {
            var response = await _walletUseCase.WithdrawAsync(CurrentUser, request.Amount);
            return ApiOk(response, "Rút tiền thành công.");
        }
    }
}
