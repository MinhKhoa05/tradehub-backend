namespace TradeHub.DAL.Entities
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;

        public string NomalizedName { get; set; } = null!;
        public string? Description { get; set; }
        public double Price { get; set; }
        public int Stock { get; set; } = 0;
        public int SellerId { get; set; }

    }
}
