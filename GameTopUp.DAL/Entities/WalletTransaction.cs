using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GameTopUp.DAL.Entities
{
    [Table("wallet_transactions")]
    public class WalletTransaction
    {
        [Key]
        public long Id { get; set; }
        public long UserId { get; set; }
        public decimal Amount { get; set; }
        public decimal BalanceBefore { get; set; }
        public decimal BalanceAfter { get; set; }
        public WalletTransactionType Type { get; set; }
        public string? Description { get; set; }
        public long? OrderId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    public enum WalletTransactionType
    {
        Deposit = 1,
        Withdraw = 2,
        PaidOrder = 3,
        Refund = 4,
    }
}
