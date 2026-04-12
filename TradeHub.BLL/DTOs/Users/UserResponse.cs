namespace TradeHub.BLL.DTOs.Users
{
    public class UserResponse
    {
        public long Id { get; set; }
        public string Name { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Phone { get; set; } = null!;
        public string Address { get; set; } = null!;
        public string AvatarUrl { get; set; } = null!;
    }
}
