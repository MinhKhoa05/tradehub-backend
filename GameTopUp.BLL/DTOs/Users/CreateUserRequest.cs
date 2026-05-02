using System.ComponentModel.DataAnnotations;

namespace GameTopUp.BLL.DTOs.Users
{
    public class CreateUserRequest
    {
        [Required(ErrorMessage = "T�n kh�ng du?c d? tr?ng")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "T�n ph?i t? 3 -> 50 k� t?")]
        public string Name { get; set; } = null!;

        [Required(ErrorMessage = "Email kh�ng du?c d? tr?ng")]
        [EmailAddress(ErrorMessage = "Email kh�ng h?p l?")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "M?t kh?u kh�ng du?c d? tr?ng")]
        [MinLength(8, ErrorMessage = "M?t kh?u ph?i �t nh?t 8 k� t?")]
        public string Password { get; set; } = null!;

        // public string Phone { get; set; } = null!;
        // public string Address { get; set; } = null!;
        // public string AvatarUrl { get; set; } = null!;
    }
}
