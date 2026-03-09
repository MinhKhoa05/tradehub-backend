namespace TradeHub.BLL.DTOs.Carts
{
    public class CartItemDTO
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }

        public CartProductDTO? Product { get; set; }
    }
}
