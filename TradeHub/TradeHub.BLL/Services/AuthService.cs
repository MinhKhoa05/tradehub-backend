using TradeHub.BLL.DTOs;
using TradeHub.BLL.DTOs.Auths;
using TradeHub.BLL.Exceptions;
using TradeHub.DAL.Entities;
using TradeHub.BLL.DTOs.Users;

namespace TradeHub.BLL.Services
{
    public class AuthService
    {
        private readonly UserService _userService;
        private readonly TokenService _tokenService;

        public AuthService(UserService userService, TokenService tokenService)
        {
            _userService = userService;
            _tokenService = tokenService;
        }

        public async Task RegisterAsync(CreateUserRequest request)
        {
            // Kiểm tra mật khẩu và hash
            if (!IsStrongPassword(request.Password))
            {
                throw new BusinessException("Mật khẩu phải có ít nhất 8 ký tự, 1 chữ hoa, 1 chữ thường, 1 số");
            }
            
            request.Password = HashPassword(request.Password);

            await _userService.CreateUserAsync(request);
        }

        public async Task<string> LoginAsync(LoginRequest loginRequest)
        {
            var user = await _userService.GetByEmailAsync(loginRequest.Email);
            if (user == null)
            {
                throw new UnauthorizedAccessException("Email hoặc mật khẩu không đúng");
            }
            
            // Kiểm tra mật khẩu có đúng không
            bool verify = VerifyPassword(loginRequest.Password, user.PasswordHash);
            if (!verify)
            {
                throw new UnauthorizedAccessException("Email hoặc mật khẩu không đúng");
            }

            // Tạo JWT AccessToken
            var tokenRequest = new TokenRequest
            {
                UserId = user.Id,
                Email = user.Email,
                Name = user.Name
            };

            return _tokenService.GenerateAccessToken(tokenRequest);
        }

        public async Task ChangePasswordAsync(int userId, PasswordChangeRequest passwordChangeRequest)
        {
            if (passwordChangeRequest.CurrentPassword == passwordChangeRequest.NewPassword)
            {
                throw new BusinessException("Mật khẩu mới không được trùng mật khẩu hiện tại");
            }

            var user = await _userService.GetUserByIdOrThrowAsync(userId);

            // VerfyPassword
            bool verify = VerifyPassword(passwordChangeRequest.CurrentPassword, user.PasswordHash);
            if (!verify)
            {
                throw new BusinessException("Mật khẩu hiện tại không đúng");
            }

            // Kiểm tra mật khẩu và hash
            if (!IsStrongPassword(passwordChangeRequest.NewPassword))
            {
                throw new BusinessException("Mật khẩu phải có ít nhất 8 ký tự, 1 chữ hoa, 1 chữ thường, 1 số");
            }

            string passwordHash = HashPassword(passwordChangeRequest.NewPassword);

            await _userService.UpdatePasswordAsync(userId, passwordHash);
        }

        public async Task<User?> GetMe(int userId)
        {
            return await _userService.GetUserByIdOrThrowAsync(userId);
        }

        private static string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        private static bool VerifyPassword(string password, string passwordHash)
        {
            return BCrypt.Net.BCrypt.Verify(password, passwordHash);
        }

        private static bool IsStrongPassword(string password)
        {
            if (password.Length < 8)
                return false;
            if (!password.Any(char.IsUpper))
                return false;
            if (!password.Any(char.IsLower))
                return false;
            if (!password.Any(char.IsDigit))
                return false;
            if (!password.Any(ch => !char.IsLetterOrDigit(ch)))
                return false;
            return true;
        }
    }
}
