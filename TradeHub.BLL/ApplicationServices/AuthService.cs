using TradeHub.BLL.DTOs.Auths;
using TradeHub.BLL.Exceptions;
using TradeHub.DAL.Entities;
using TradeHub.BLL.DTOs.Users;
using TradeHub.BLL.Services;

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
            // Kiểm tra mật khẩu và hash
            _password.Validate(request.Password);

            request.Password = _password.Hash(request.Password);

            await _user.CreateUserAsync(request);
        }

        public async Task<string> LoginAsync(LoginRequest request)
        {
            var user = await _user.GetByEmailAsync(request.Email)
                        ?? throw new BusinessException("Email hoặc mật khấu không đúng");
            
            // Kiểm tra mật khẩu có đúng không
            if (!_password.Verify(request.Password, user.PasswordHash))
            {
                throw new BusinessException("Email hoặc mật khẩu không đúng");
            }

            // Tạo JWT AccessToken
            var tokenRequest = new TokenRequest
            {
                UserId = user.Id,
                Email = user.Email,
                Name = user.Name
            };

            return _token.GenerateAccessToken(tokenRequest);
        }

        public async Task ChangePasswordAsync(int userId, PasswordChangeRequest request)
        {
            if (request.CurrentPassword == request.NewPassword)
            {
                throw new BusinessException("Mật khẩu mới không được trùng mật khẩu hiện tại");
            }

            // Kiểm tra mật khẩu mạnh
            _password.Validate(request.NewPassword);

            var user = await _user.GetUserByIdOrThrowAsync(userId);

            // VerfyPassword
            if (!_password.Verify(request.CurrentPassword, user.PasswordHash))
            {
                throw new BusinessException("Mật khẩu hiện tại không đúng");
            }

            string passwordHash = _password.Hash(request.NewPassword);

            await _user.UpdatePasswordAsync(userId, passwordHash);
        }

        public async Task<User> GetCurrentUserAsync(int userId)
        {
            return await _user.GetUserByIdOrThrowAsync(userId);
        }
    }
}
