using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TradeHub.DAL.Entities
{
    [Table("orders")]
    public class Order
    {
        [Key]
        public long Id { get; set; }

        public long BuyerId { get; set; }
        public long SellerId { get; set; }

        public int TotalAmount { get; set; }

        public PaymentMethod PaymentMethod { get; set; }

        public OrderStatus Status { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public enum OrderStatus
    {
        Pending = 1,        // user tạo đơn
        Confirmed = 2,      // shop xác nhận
        ReadyForPickup = 3, // hàng sẵn sàng
        Delivering = 4,     // shipper đã lấy và đang giao
        Delivered = 5,      // shipper giao xong
        Completed = 6,      // user xác nhận / auto confirm
        Cancelled = 7,      // hủy đơn hàng
        DeliveryFailed = 8  // giao hàng thất bại (shipper)
    }

    public enum PaymentMethod
    {
        Cod = 1,
        Wallet = 2
    }
}
