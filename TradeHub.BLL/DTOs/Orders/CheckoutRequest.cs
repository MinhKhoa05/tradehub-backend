namespace TradeHub.BLL.DTOs.Orders
{
    public class CheckoutRequest
    {
        public long CartId { get; set; }
        public string GameAccountInfo { get; set; } = null!;
    }
}
