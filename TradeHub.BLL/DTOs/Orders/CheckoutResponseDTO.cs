namespace TradeHub.BLL.DTOs.Orders
{
    public class CheckoutResponseDTO
    {
        public List<long> OrderIds { get; set; } = new List<long>();
        public long TransactionId { get; set; }
    }
}
