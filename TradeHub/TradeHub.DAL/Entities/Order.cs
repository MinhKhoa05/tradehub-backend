namespace TradeHub.DAL.Entities
{
    public class Order
    {
        public int Id { get; set; }

        public int BuyerId { get; set; }
        public int SellerId { get; set; }

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
