using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TradeHub.DAL.Entities
{
    [Table("wallets")]
    public class Wallet
    {
        [Key]
        public long Id { get; set; }
        public long UserId { get; set; }
        public long Balance { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
