namespace TradeHub.DAL.Entities
{
    public class Game
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string ImageUrl { get; set; }
        public bool IsActive { get; set; } = true;

        // Tracking nếu có
    }
}
