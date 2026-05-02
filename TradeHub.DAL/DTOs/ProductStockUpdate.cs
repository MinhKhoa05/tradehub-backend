namespace TradeHub.DAL.DTOs
{
    public class ProductStockUpdate
    {
        public int Id { get; set; }
        public int Quantity { get; set; }

        public ProductStockUpdate(int id, int quantity)
        {
            Id = id;
            Quantity = quantity;
        }
    }
}
