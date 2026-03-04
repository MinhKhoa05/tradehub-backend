using System.ComponentModel.DataAnnotations;

namespace TradeHub.BLL.DTOs
{
    public class LoginRequest
    {
        [Required(ErrorMessage = "Email không được để trống")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]

        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "Mật khẩu không được để trống")]
        [MinLength(8, ErrorMessage = "Mật khẩu phải ít nhất 8 ký tự")]
        public string Password { get; set; } = null!;
    }
}
