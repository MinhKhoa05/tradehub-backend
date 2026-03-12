using TradeHub.BLL.DTOs.Users;
using TradeHub.BLL.Exceptions;
using TradeHub.DAL.Entities;
using TradeHub.DAL.Repositories;

namespace TradeHub.BLL.Services
{
    public class UserService
    {
        private readonly UserRepository _userRepo;
        
        public UserService(UserRepository userRepo)
        {
            _userRepo = userRepo;
        }
        
        public async Task<int> CreateUserAsync(CreateUserRequest request)
        {
            // Kiểm tra email đã được đăng ký chưa?
            var existingUser = await _userRepo.GetByEmailAsync(request.Email);
            if (existingUser != null)
                throw new BusinessException("Email đã được sử dụng");

            // Thêm user mới
            var user = new User
            {
                Name = request.Name,
                Email = request.Email,
                PasswordHash = request.Password,
            };

            return await _userRepo.CreateAsync(user);
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _userRepo.GetByEmailAsync(email);
        }

        public async Task UpdatePasswordAsync(int userId, string newPasswordHash)
        {
            await _userRepo.UpdatePasswordAsync(userId, newPasswordHash);
        }

        public async Task<User> GetUserByIdOrThrowAsync(int userId)
        {
            var user = await _userRepo.GetByIdAsync(userId)
                            ?? throw new BusinessException("Người dùng không tồn tại");
            return user;
        }
    }
}
