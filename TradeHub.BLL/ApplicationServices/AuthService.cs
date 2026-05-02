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
            request.Password = _password.Hash(request.Password);
            await _user.RegisterAsync(request);
        }

        public async Task<LoginResponseDTO> LoginAsync(LoginRequest request)
        {
            var user = await _user.GetByEmailAsync(request.Email);
            
            if (user == null || !_password.Verify(request.Password, user.PasswordHash))
            {
                throw new BusinessException("Email hoặc mật khẩu không chính xác.");
            }

            var token = _token.GenerateAccessToken(new TokenRequest
            {
                UserId = user.Id,
                Email = user.Email,
                Name = user.Username,
                Role = user.Role.ToString()
            });

            return new LoginResponseDTO { AccessToken = token };
        }

        public async Task ChangePasswordAsync(UserContext context, PasswordChangeRequest request)
        {
            if (request.CurrentPassword == request.NewPassword)
            {
                throw new BusinessException("Mật khẩu mới không được trùng với mật khẩu hiện tại.");
            }
            
            _password.Validate(request.NewPassword);

            var user = await _user.GetProfileAsync(context.UserId);
            if (!_password.Verify(request.CurrentPassword, user.PasswordHash))
            {
                throw new BusinessException("Mật khẩu hiện tại không chính xác.");
            }

            await _user.ChangePasswordAsync(context.UserId, _password.Hash(request.NewPassword));
        }
    }
}
