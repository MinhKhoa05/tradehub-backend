namespace TradeHub.BLL.DTOs.Orders
{
    public class CheckoutItem
    {
        public int SellerId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public int UnitPrice { get; set; }
    }
}
