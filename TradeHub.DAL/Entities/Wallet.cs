using Dapper.Contrib.Extensions;

namespace TradeHub.DAL.Entities
{
    [Table("wallets")]
    public class Wallet
    {
        [Key]
        public long Id { get; set; }
        public long UserId { get; set; }
        public int Balance { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
