using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GameTopUp.BLL.Services;
using GameTopUp.BLL.DTOs.Wallets;

namespace GameTopUp.API.Controllers
{
    [Authorize]
    [Route("api/wallet")]
    [ApiController]
    public class WalletController : ApiControllerBase
    {
        private readonly WalletService _wallet;

        public WalletController(WalletService wallet)
        {
            _wallet = wallet;
        }

        [HttpPost("active")]
        public async Task<IActionResult> CreateWallet()
        {
            var walletId = await _wallet.CreateWalletAsync(CurrentUser);
            return ApiOk(new { WalletId = walletId }, "Kích hoạt ví thành công.");
        }

        [HttpGet]
        public async Task<IActionResult> GetBalance()
        {
            var balance = await _wallet.GetBalanceAsync(CurrentUser);
            return ApiOk(balance);
        }

        [HttpGet("transactions")]
        public async Task<IActionResult> GetWalletTransactions()
        {
            var transactions = await _wallet.GetTransactionsAsync(CurrentUser);
            return ApiOk(transactions);
        }

        [HttpPost("transactions/deposit")]
        public async Task<IActionResult> Deposit([FromBody] WalletTransactionRequest request)
        {
            var response = await _wallet.DepositAsync(CurrentUser, request.Amount);
            return ApiOk(response, "Nạp tiền thành công.");
        }

        [HttpPost("transactions/withdraw")]
        public async Task<IActionResult> Withdraw([FromBody] WalletTransactionRequest request)
        {
            var response = await _wallet.WithdrawAsync(CurrentUser, request.Amount);
            return ApiOk(response, "Rút tiền thành công.");
        }
    }
}
