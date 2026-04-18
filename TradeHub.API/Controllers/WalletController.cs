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
        public async Task<IActionResult> GetMyBalance()
        {
            var wallet = await _wallet.GetMyBalanceAsync();
            return ApiOk(wallet);
        }

        [HttpGet("transactions")]
        public async Task<IActionResult> GetWalletTransaction()
        {
            var transactions = await _wallet.GetMyTransactionsAsync();
            return ApiOk(transactions);
        }

        [HttpPost("transactions/deposit")]
        public async Task<IActionResult> Deposit([FromBody] int amount)
        {
            var transaction = await _wallet.DepositAsync(amount);
            return ApiOk(transaction);
        }

        [HttpPost("transactions/withdraw")]
        public async Task<IActionResult> Withdraw([FromBody] int amount)
        {
            var transaction = await _wallet.WithdrawAsync(amount);
            return ApiOk(transaction);
        }

        [HttpPost("pay")]
        public async Task<IActionResult> Pay([FromBody] PayRequest request)
        {
            var transaction = await _wallet.PayForOrdersAsync(request.OrderIds, request.TotalAmount);
            return ApiOk(transaction);
        }
    }

    public class PayRequest
    {
        public List<long> OrderIds { get; set; } = new();
        public int TotalAmount { get; set; }
    }
}
