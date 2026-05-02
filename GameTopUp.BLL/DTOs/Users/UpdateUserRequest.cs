using GameTopUp.DAL.Entities;

namespace GameTopUp.BLL.DTOs.Users
{
    public class UpdateUserRequest
    {
        public string? Username { get; set; }
        public string? Email { get; set; }
        public UserRole? Role { get; set; }
        public bool? IsActive { get; set; }
    }
}
