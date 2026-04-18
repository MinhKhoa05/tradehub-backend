using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TradeHub.DAL.Entities
{
    [Table("order_items")]
    public class OrderItem
    {
        [Key]
        public long Id { get; set; }
        public long OrderId { get; set; }
        public long ProductId { get; set; }
        public int UnitPrice { get; set; }
        public int Quantity { get; set; }

        public int LineTotal => UnitPrice * Quantity;
    }
}
