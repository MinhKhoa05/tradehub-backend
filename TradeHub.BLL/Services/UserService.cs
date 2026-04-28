using TradeHub.BLL.Common;
using TradeHub.BLL.DTOs.Users;
using TradeHub.BLL.Exceptions;
using TradeHub.DAL.Entities;
using TradeHub.DAL.Repositories;

namespace TradeHub.BLL.Services
{
    public class UserService : BaseService
    {
        private readonly UserRepository _userRepo;

        public UserService(UserRepository userRepo, IIdentityService identityService)
            : base(identityService)
        {
            _userRepo = userRepo;
        }

        // ===== PUBLIC / AUTH (Đăng ký & Tìm kiếm chung) =====

        public async Task<long> RegisterAsync(CreateUserRequest request)
        {
            var existingUser = await _userRepo.GetByEmailAsync(request.Email);

            if (existingUser != null)
                throw new BusinessException("Email này đã được sử dụng.");

            var user = new User
            {
                Username = request.Name, // Map Name sang Username
                Email = request.Email,
                PasswordHash = request.Password, // Lưu ý: Cần Hash mật khẩu trong thực tế
                CreatedAt = DateTime.UtcNow
            };

            return await _userRepo.CreateAsync(user);
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _userRepo.GetByEmailAsync(email);
        }

        // ===== PERSONAL (Hành động của Tôi) =====

        public async Task<User> GetMyProfileAsync()
        {
            return await GetByIdInternalAsync(CurrentUserId);
        }

        public async Task ChangePasswordAsync(string newPasswordHash)
        {
            await _userRepo.UpdatePasswordAsync(CurrentUserId, newPasswordHash);
        }

        // ===== INTERNAL (Xử lý nội bộ) =====
        private async Task<User> GetByIdInternalAsync(long userId)
        {
            return await _userRepo.GetByIdAsync(userId)
                ?? throw new BusinessException("Người dùng không tồn tại trong hệ thống.");
        }
    }
}