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
        public OrderStatus ToStatus { get; set; } // Trạng thái mới được cập nhật

        public string? Note { get; set; } // Lý do thay đổi (ví dụ: "Sai ID game", "Đã hoàn tiền")

        public long ActionBy { get; set; } // Id của người thực hiện (Admin hoặc User)

        public bool IsAdmin { get; set; } // Flag nhanh để biết ai đổi: True = Admin, False = User

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}