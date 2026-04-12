using Dapper.Contrib.Extensions;

namespace TradeHub.DAL.Entities
{
    [Table("wallet_transactions")]
    public class WalletTransaction
    {
        [Key]
        public long Id { get; set; }
        public long WalletId { get; set; }

        // > 0: Tiền vào;   < 0 tiền ra
        public int Amount { get; set; }
        public WalletTransactionType Type { get; set; }

        // Order Id nếu thanh toán đơn hàng ,...
        public int? ReferenceId { get; set; }

        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public enum WalletTransactionType
    {
        Deposit = 1,
        Withdraw = 2,
        PaidOrder = 3,
        Refund = 4,
    }
}