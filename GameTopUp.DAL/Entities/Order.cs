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

        public long WalletTransactionId { get; set; }

        public long GamePackageId { get; set; }
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }

        public decimal Total => UnitPrice * Quantity;

        public long AssignTo { get; set; } // Admin dang nh?n x? l�
        public DateTime AssignAt { get; set; } // Th?i di?m admin nh?n x? l�

        public OrderStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public enum OrderStatus
    {
        Pending = 1,        // User d?t h�ng
        Processing = 2,     // Admin dang x? l�
        Completed = 3,      // Ho�n th�nh
        Canacelled = 4      // H?y don
    }
}
