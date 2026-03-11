using TradeHub.BLL.DTOs.Auths;
using TradeHub.BLL.Exceptions;
using TradeHub.DAL.Entities;
using TradeHub.BLL.DTOs.Users;
using TradeHub.BLL.Services;
using TradeHub.DAL;

namespace TradeHub.BLL.ApplicationServices
{
    public class AuthService
    {
        private readonly UserService _userService;
        private readonly TokenService _tokenService;
        private readonly WalletService _walletService;
        private readonly DatabaseContext _databaseContext;

        public AuthService(UserService userService, TokenService tokenService, WalletService walletService, DatabaseContext databaseContext)
        {
            _userService = userService;
            _tokenService = tokenService;
            _walletService = walletService;
            _databaseContext = databaseContext;
        }

        public async Task RegisterAsync(CreateUserRequest request)
        {
            // Kiểm tra mật khẩu và hash
            ValidatePassword(request.Password);
            
            request.Password = HashPassword(request.Password);

            await _databaseContext.ExecuteInTransactionAsync(async () =>
            {
                
                var userId = await _userService.CreateUserAsync(request);

                // tạo ví cho user
                await _walletService.CreateWalletAsync(userId);
            });
        }

        public async Task<string> LoginAsync(LoginRequest loginRequest)
        {
            var user = await _userService.GetByEmailAsync(loginRequest.Email);
            if (user == null)
            {
                throw new BusinessException("Email hoặc mật khẩu không đúng");
            }
            
            // Kiểm tra mật khẩu có đúng không
            bool verify = VerifyPassword(loginRequest.Password, user.PasswordHash);
            if (!verify)
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
            ValidatePassword(passwordChangeRequest.NewPassword);

            string passwordHash = HashPassword(passwordChangeRequest.NewPassword);

            await _userService.UpdatePasswordAsync(userId, passwordHash);
        }

        public async Task<User?> GetCurrentUserAsync(int userId)
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

        private static void ValidatePassword(string password)
        {
            if (!IsStrongPassword(password))
            {
                throw new BusinessException("Mật khẩu phải có ít nhất 8 ký tự, gồm chữ hoa, chữ thường, số và ký tự đặc biệt");
            }
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
