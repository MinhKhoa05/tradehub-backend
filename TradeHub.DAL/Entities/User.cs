using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TradeHub.DAL.Entities
{
    [Table("users")]
    public class User
    {
        [Key]
        public long Id { get; set; }

        public string Username { get; set; } = null!; // Nickname hiển thị cho người dùng

        public string Email { get; set; } = null!; // Dùng để đăng nhập

        public string PasswordHash { get; set; } = null!;

        public decimal Balance { get; set; } = 0;

        public UserRole Role { get; set; } = UserRole.Member;

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }

    public enum UserRole
    {
        Member = 0,
        Admin = 1,
        Staff = 2 // Nhân viên nạp thuê
    }
}
