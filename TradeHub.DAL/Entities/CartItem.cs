using Dapper.Contrib.Extensions;

namespace TradeHub.DAL.Entities
{
    [Table("cart_items")]
    public class CartItem
    {
        [Key]
        public long Id { get; set; }
        public long UserId { get; set; }
        public long ProductId { get; set; }
        public int Quantity { get; set; }
    }
}
