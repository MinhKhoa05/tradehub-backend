using System.ComponentModel.DataAnnotations;

namespace TradeHub.BLL.DTOs.Users
{
    public class CreateUserRequest
    {
        [Required(ErrorMessage = "Tên không được để trống")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Tên phải từ 3 -> 50 ký tự")]
        public string Name { get; set; } = null!;

        [Required(ErrorMessage = "Email không được để trống")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "Mật khẩu không được để trống")]
        [MinLength(8, ErrorMessage = "Mật khẩu phải ít nhất 8 ký tự")]
        public string Password { get; set; } = null!;
    }
}
