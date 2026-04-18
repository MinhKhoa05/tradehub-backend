using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TradeHub.DAL.Entities
{
    [Table("products")]
    public class Product
    {
        [Key]
        public long Id { get; set; }
        public string Name { get; set; } = null!;

        public string NormalizedName { get; set; } = null!;
        public string? Description { get; set; }
        public int Price { get; set; }
        public int Stock { get; set; } = 0;
        public long SellerId { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
