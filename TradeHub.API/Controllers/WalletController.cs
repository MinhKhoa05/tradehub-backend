using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TradeHub.BLL.Services;
using TradeHub.BLL.DTOs.Wallets;

namespace TradeHub.API.Controllers
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

        [HttpPost]
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
        public async Task<IActionResult> Deposit([FromBody] int amount)
        {
            var response = await _wallet.DepositAsync(CurrentUser, amount);
            return ApiOk(response, "Nạp tiền thành công.");
        }

        [HttpPost("transactions/withdraw")]
        public async Task<IActionResult> Withdraw([FromBody] int amount)
        {
            var response = await _wallet.WithdrawAsync(CurrentUser, amount);
            return ApiOk(response, "Rút tiền thành công.");
        }

        [HttpPost("pay")]
        public async Task<IActionResult> Pay([FromBody] PayRequest request)
        {
            var response = await _wallet.PayForOrdersAsync(CurrentUser, request.OrderIds, request.TotalAmount);
            return ApiOk(response, "Thanh toán đơn hàng thành công.");
        }
    }

    public class PayRequest
    {
        public List<long> OrderIds { get; set; } = new();
        public int TotalAmount { get; set; }
    }
}
