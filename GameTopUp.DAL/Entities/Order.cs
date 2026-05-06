using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GameTopUp.DAL.Entities
{
    [Table("orders")]
    public class Order
    {
        [Key]
        public long Id { get; set; }
        
        public long UserId { get; set; }
        public string GameAccountInfo { get; set; } = null!;

        public long GamePackageId { get; set; }
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }

        [NotMapped]
        public decimal Total => UnitPrice * Quantity;

        public long AssignTo { get; set; } // Admin đang nhận xử lý
        public DateTime AssignAt { get; set; } // Thời điểm admin nhận xử lý

        public OrderStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public enum OrderStatus
    {
        Pending = 1,        // Chờ thanh toán
        Paid = 2,           // Đã thanh toán, chờ xử lý
        Processing = 3,     // Admin đang xử lý
        Completed = 4,      // Hoàn thành
        Cancelled = 5       // Hủy đơn
    }
}
