using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TradeHub.BLL.Services;

namespace TradeHub.API.Controllers
{
    [Authorize]
    [Route("api/wallet")]
    [ApiController]
    public class WalletController : BaseController
    {
        private readonly WalletService _wallet;

        public WalletController(WalletService wallet)
        {
            _wallet = wallet;
        }

        [HttpGet]
        public async Task<IActionResult> GetWallet()
        {
            var wallet = await _wallet.GetWalletByUserIdOrThrowAsync(CurrentUserId);
            return ApiOk(wallet);
        }

        [HttpGet("transactions")]
        public async Task<IActionResult> GetWalletTransaction()
        {
            var transactions = await _wallet.GetWalletTransactionsAsync(CurrentUserId);
            return ApiOk(transactions);
        }

        [HttpPost("transactions/deposit")]
        public async Task<IActionResult> Deposit([FromBody] int amount)
        {
            var transaction = await _wallet.DepositAsync(CurrentUserId, amount);
            return ApiOk(transaction);
        }

        [HttpPost("transactions/withdraw")]
        public async Task<IActionResult> Withdraw([FromBody] int amount)
        {
            var transaction = await _wallet.WithdrawAsync(CurrentUserId, amount);
            return ApiOk(transaction);
        }
    }
}
