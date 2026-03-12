using TradeHub.DAL.Entities;

namespace TradeHub.BLL.DTOs.Orders
{
    public class UpdateOrderStatusRequest
    {
        public ActorType ActorType { get; set; }
        public OrderStatus ToStatus { get; set; }
        public string? Note { get; set; }
    }
}
