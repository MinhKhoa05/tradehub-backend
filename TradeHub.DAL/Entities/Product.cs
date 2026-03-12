namespace TradeHub.DAL.Entities
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;

        public string NormalizedName { get; set; } = null!;
        public string? Description { get; set; }
        public int Price { get; set; }
        public int Stock { get; set; } = 0;
        public int SellerId { get; set; }

    }
}
