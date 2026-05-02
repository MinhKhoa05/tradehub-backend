using System.ComponentModel.DataAnnotations;

namespace GameTopUp.BLL.DTOs.Auths
{
    public class LoginRequest
    {
        [Required(ErrorMessage = "Email kh�ng du?c d? tr?ng")]
        [EmailAddress(ErrorMessage = "Email kh�ng h?p l?")]

        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "M?t kh?u kh�ng du?c d? tr?ng")]
        [MinLength(8, ErrorMessage = "M?t kh?u ph?i �t nh?t 8 k� t?")]
        public string Password { get; set; } = null!;
    }
}
