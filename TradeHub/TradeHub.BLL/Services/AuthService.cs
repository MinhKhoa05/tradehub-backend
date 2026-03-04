using TradeHub.BLL.DTOs;
using TradeHub.BLL.Exceptions;
using TradeHub.DAL.Entities;
using TradeHub.DAL.Repositories;

namespace TradeHub.BLL.Services
{
    public class AuthService
    {
        private readonly UserRepository _userRepository;
        private readonly TokenService _tokenService;

        public AuthService(UserRepository userRepository, TokenService tokenService)
        {
            _userRepository = userRepository;
            _tokenService = tokenService;
        }

        public async Task Register(UserRegisterRequest userRegisterRequest)
        {
            // Kiểm tra email đã được đăng ký chưa?
            var existingUser = await _userRepository.GetByEmailAsync(userRegisterRequest.Email);
            if (existingUser != null)
            {
                throw new BusinessException("Email đã được sử dụng");
            }

            // Kiểm tra mật khẩu và hash
            if (!IsStrongPassword(userRegisterRequest.Password))
            {
                throw new BusinessException("Mật khẩu phải có ít nhất 8 ký tự, 1 chữ hoa, 1 chữ thường, 1 số");
            }
            
            string passwordHash = HashPassword(userRegisterRequest.Password);

            // Thêm user mới
            var user = new User
            {
                Name = userRegisterRequest.Name,
                Email = userRegisterRequest.Email,
                PasswordHash = passwordHash,
                Balance = 0
            };
            
            await _userRepository.CreateAsync(user);
        }

        public async Task<string> Login(LoginRequest loginRequest)
        {
            // Kiểm tra user có tồn tại không
            var user = await _userRepository.GetByEmailAsync(loginRequest.Email);
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

        public async Task ChangePassword(int userId, PasswordChangeRequest passwordChangeRequest)
        {
            if (passwordChangeRequest.CurrentPassword == passwordChangeRequest.NewPassword)
            {
                throw new BusinessException("Mật khẩu mới không được trùng mật khẩu hiện tại");
            }

            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                throw new NotFoundException("User", "id", userId);
            }

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

            await _userRepository.UpdatePasswordAsync(userId, passwordHash);
        }

        public async Task<UserResponse> GetMe(int userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                throw new NotFoundException("User", "id", userId);
            }

            return new UserResponse
            {
                Id = userId,
                Name = user.Name,
                Email = user.Email,
                Balance = user.Balance
            };
        }

        private string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        private bool VerifyPassword(string password, string passwordHash)
        {
            return BCrypt.Net.BCrypt.Verify(password, passwordHash);
        }

        private bool IsStrongPassword(string password)
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
