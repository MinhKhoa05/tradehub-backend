namespace TradeHub.BLL.DTOs
{
    public class UserResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Email { get; set; }
        public int Balance { get; set; }
    }
}
