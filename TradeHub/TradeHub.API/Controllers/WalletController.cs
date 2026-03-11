using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TradeHub.BLL.Services;
using TradeHub.API.Extensions;

namespace TradeHub.API.Controllers
{
    [Route("api/wallet")]
    [ApiController]
    public class WalletController : ControllerBase
    {
        private readonly WalletService _walletService;

        public WalletController(WalletService walletService)
        {
            _walletService = walletService;
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetMyWallet()
        {
            var userId = HttpContext.GetUserId();
            var wallet = await _walletService.GetWalletByUserIdOrThrowAsync(userId);
            return Ok(wallet);
        }

        [Authorize]
        [HttpGet("transactions")]
        public async Task<IActionResult> GetWalletTransaction()
        {
            var userId = HttpContext.GetUserId();
            var transactions = await _walletService.GetWalletTransactionsAsync(userId);
            return Ok(transactions);
        }

        [Authorize]
        [HttpPost("deposit")]
        public async Task<IActionResult> Deposit([FromBody] int amount)
        {
            var userId = HttpContext.GetUserId();
            var transaction = await _walletService.DepositAsync(userId, amount);
            return Ok(transaction);
        }

        [Authorize]
        [HttpPost("withdraw")]
        public async Task<IActionResult> Withdraw([FromBody] int amount)
        {
            var userId = HttpContext.GetUserId();
            var transaction = await _walletService.WithdrawAsync(userId, amount);
            return Ok(transaction);
        }
    }
}
