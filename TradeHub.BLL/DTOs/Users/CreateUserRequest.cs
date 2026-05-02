using System.ComponentModel.DataAnnotations;

namespace TradeHub.BLL.DTOs.Users
{
    public class CreateUserRequest
    {
        [Required(ErrorMessage = "Tên không du?c d? tr?ng")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Tên ph?i t? 3 -> 50 ký t?")]
        public string Name { get; set; } = null!;

        [Required(ErrorMessage = "Email không du?c d? tr?ng")]
        [EmailAddress(ErrorMessage = "Email không h?p l?")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "M?t kh?u không du?c d? tr?ng")]
        [MinLength(8, ErrorMessage = "M?t kh?u ph?i ít nh?t 8 ký t?")]
        public string Password { get; set; } = null!;

        // public string Phone { get; set; } = null!;
        // public string Address { get; set; } = null!;
        // public string AvatarUrl { get; set; } = null!;
    }
}
