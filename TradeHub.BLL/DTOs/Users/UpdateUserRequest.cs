using TradeHub.DAL.Entities;

namespace TradeHub.BLL.DTOs.Users
{
    public class UpdateUserRequest
    {
        public string? Username { get; set; }
        public string? Email { get; set; }
        public UserRole? Role { get; set; }
        public bool? IsActive { get; set; }
    }
}
