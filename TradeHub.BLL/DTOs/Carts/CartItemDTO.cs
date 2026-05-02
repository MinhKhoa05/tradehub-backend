namespace TradeHub.BLL.DTOs.Carts
{
    public class CartItemDTO
    {
        public long Id { get; set; }
        public long ProductId { get; set; }
        public long Quantity { get; set; }

        public CartProductDTO? Product { get; set; }
    }
}
