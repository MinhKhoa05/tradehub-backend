using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace TradeHub.DAL.Entities
{
    [Table("order_history")]
    public class OrderHistory
    {
        [Key]
        public long Id { get; set; }

        public long OrderId { get; set; }


        public OrderStatus FromStatus { get; set; }
        public OrderStatus ToStatus { get; set; } // Tr?ng thái m?i du?c c?p nh?t

        public string? Note { get; set; } // Lý do thay d?i (ví d?: "Sai ID game", "Ðã hoàn ti?n")

        public long ActionBy { get; set; } // Id c?a ngu?i th?c hi?n (Admin ho?c User)

        public bool IsAdmin { get; set; } // Flag nhanh d? bi?t ai d?i: True = Admin, False = User

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
