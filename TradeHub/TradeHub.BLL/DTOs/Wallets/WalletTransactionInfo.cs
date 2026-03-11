using TradeHub.DAL.Entities;

namespace TradeHub.BLL.DTOs.Wallets
{
    public class WalletTransactionInfo
    {
        public WalletTransactionType Type { get; set; }
        public string? Description { get; set; }
        public int? ReferenceId { get; set; }
        public bool IsDecreaseBalance { get; set; } = false;
    }
}
