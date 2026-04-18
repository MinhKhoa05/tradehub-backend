using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace TradeHub.DAL.Entities
{
    [Table("order_history")]
    public class OrderHistory
    {
        [Key]
        public long Id { get; set; }
        public long OrderId { get; set; }

        public OrderStatus? FromStatus { get; set; }
        public OrderStatus ToStatus { get; set; }
        public long ChangedBy { get; set; } // UserId
        public ActorType ActorType { get; set; }
        public string? Note { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public enum ActorType
    {
        Seller = 0,
        Buyer = 1,
        Shipper = 2,
        System = 3
    }
}
