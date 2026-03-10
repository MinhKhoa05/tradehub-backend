using TradeHub.DAL.Entities;

namespace TradeHub.BLL.DTOs.Orders
{
    public class CheckoutRequest
    {
        public PaymentMethod PaymentMethod { get; set; }
        public List<CheckoutItem> Items { get; set; }
    }
}
