using TradeHub.BLL.Common;
using TradeHub.BLL.DTOs.Auths;
using TradeHub.BLL.DTOs.Users;
using TradeHub.BLL.Exceptions;
using TradeHub.BLL.Services;
using TradeHub.DAL.Entities;

namespace TradeHub.BLL.ApplicationServices
{
    public class AuthService
    {
        private readonly UserService _user;
        private readonly TokenService _token;
        private readonly PasswordService _password;

        public AuthService(UserService user, TokenService token, PasswordService password)
        {
            _user = user;
            _token = token;
            _password = password;
        }

        public async Task RegisterAsync(CreateUserRequest request)
        {
            _password.Validate(request.Password);

            // Hash mật khẩu trước khi đưa xuống tầng dưới
            var passwordHash = _password.Hash(request.Password);
            request.Password = passwordHash;

            await _user.RegisterAsync(request); // Đã sửa theo UserService mới
        }

        public async Task<string> LoginAsync(LoginRequest request)
        {
            var user = await _user.GetByEmailAsync(request.Email)
                        ?? throw new BusinessException("Email hoặc mật khẩu không đúng");

            if (!_password.Verify(request.Password, user.PasswordHash))
                throw new BusinessException("Email hoặc mật khẩu không đúng");

            return GenerateAccessTokenInternal(user);
        }

        public async Task ChangePasswordAsync(PasswordChangeRequest request)
        {
            // Bỏ 'My' vì ngữ cảnh Auth của user hiện tại đã quá rõ ràng
            if (request.CurrentPassword == request.NewPassword)
                throw new BusinessException("Mật khẩu mới không được trùng mật khẩu hiện tại");

            _password.Validate(request.NewPassword);

            // Lấy profile chính mình để verify mật khẩu cũ
            var user = await _user.GetMyProfileAsync();

            if (!_password.Verify(request.CurrentPassword, user.PasswordHash))
                throw new BusinessException("Mật khẩu hiện tại không đúng");

            var newPasswordHash = _password.Hash(request.NewPassword);

            // Gọi hàm đã refactor bên UserService
            await _user.ChangePasswordAsync(newPasswordHash);
        }

        // ===== PRIVATE / INTERNAL =====

        private string GenerateAccessTokenInternal(User user)
        {
            var tokenRequest = new TokenRequest
            {
                UserId = user.Id,
                Email = user.Email,
                Name = user.Username,
                Role = user.Role.ToString()
            };

            return _token.GenerateAccessToken(tokenRequest);
        }
    }
}