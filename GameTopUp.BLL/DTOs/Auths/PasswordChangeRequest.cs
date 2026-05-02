using System.ComponentModel.DataAnnotations;

namespace GameTopUp.BLL.DTOs.Auths
{
    public class PasswordChangeRequest
    {
        [Required(ErrorMessage = "M?t kh?u hi?n t?i kh�ng du?c d? tr?ng")]
        [MinLength(8, ErrorMessage = "M?t kh?u hi?n t?i ph?i �t nh?t 8 k� t?")]
        public string CurrentPassword { get; set; }

        [Required(ErrorMessage = "M?t kh?u m?i kh�ng du?c d? tr?ng")]
        [MinLength(8, ErrorMessage = "M?t kh?u m?i ph?i �t nh?t 8 k� t?")]
        public string NewPassword { get; set; }
    }
}
