using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GameTopUp.DAL.Entities
{
    [Table("cart_items")]
    public class CartItem
    {
        [Key]
        public long Id { get; set; }
        public long UserId { get; set; }
        public long GamePackageId { get; set; }
        public int Quantity { get; set; }
    }
}
