namespace TradeHub.DAL.Entities
{
    public class OrderHistory
    {
        public int Id { get; set; }
        public int OrderId { get; set; }

        public OrderStatus? FromStatus { get; set; }
        public OrderStatus ToStatus { get; set; }
        public int ChangedBy { get; set; } // UserId
        public ActorType ActorType { get; set; }
        public string? Note { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public enum ActorType
    {
        Seller = 0,
        Buyer = 1,
        Shipper = 2,
        System = 3
    }
}
