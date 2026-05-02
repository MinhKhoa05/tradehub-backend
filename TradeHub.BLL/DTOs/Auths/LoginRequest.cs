using System.ComponentModel.DataAnnotations;

namespace TradeHub.BLL.DTOs.Auths
{
    public class LoginRequest
    {
        [Required(ErrorMessage = "Email không du?c d? tr?ng")]
        [EmailAddress(ErrorMessage = "Email không h?p l?")]

        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "M?t kh?u không du?c d? tr?ng")]
        [MinLength(8, ErrorMessage = "M?t kh?u ph?i ít nh?t 8 ký t?")]
        public string Password { get; set; } = null!;
    }
}
