using System.ComponentModel.DataAnnotations;

namespace TradeHub.BLL.DTOs.Auths
{
    public class PasswordChangeRequest
    {
        [Required(ErrorMessage = "M?t kh?u hi?n t?i không du?c d? tr?ng")]
        [MinLength(8, ErrorMessage = "M?t kh?u hi?n t?i ph?i ít nh?t 8 ký t?")]
        public string CurrentPassword { get; set; }

        [Required(ErrorMessage = "M?t kh?u m?i không du?c d? tr?ng")]
        [MinLength(8, ErrorMessage = "M?t kh?u m?i ph?i ít nh?t 8 ký t?")]
        public string NewPassword { get; set; }
    }
}
