using GameTopUp.BLL.Exceptions;

namespace GameTopUp.BLL.Common
{
    /// <summary>
    /// PasswordService chịu trách nhiệm xử lý bảo mật mật khẩu.
    /// Sử dụng BCrypt để băm mật khẩu giúp chống lại các cuộc tấn công Brute-force và Rainbow Table.
    /// </summary>
    public class PasswordService
    {        
        public string Hash(string password)
        {
            // BCrypt tự động tạo Salt và tích hợp vào chuỗi Hash kết quả.
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        public bool Verify(string password, string passwordHash)
        {
            return BCrypt.Net.BCrypt.Verify(password, passwordHash);
        }

        /// <summary>
        /// Kiểm tra độ mạnh của mật khẩu trước khi lưu trữ.
        /// Việc ràng buộc mật khẩu mạnh giúp bảo vệ tài khoản người dùng khỏi các nguy cơ bị đánh cắp thông tin.
        /// </summary>
        public void Validate(string password)
        {
            if (!IsStrongPassword(password))
            {
                throw new BusinessException("Mật khẩu không đủ mạnh. Yêu cầu: ít nhất 8 ký tự, bao gồm chữ hoa, chữ thường, số và ký tự đặc biệt.");
            }
        }

        private bool IsStrongPassword(string password)
        {
            if (password.Length < 8) return false;
            if (!password.Any(char.IsUpper)) return false;
            if (!password.Any(char.IsLower)) return false;
            if (!password.Any(char.IsDigit)) return false;
            if (!password.Any(ch => !char.IsLetterOrDigit(ch))) return false;
            return true;
        }
    }
}
