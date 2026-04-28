using TradeHub.BLL.Common;
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

        public async Task<long> RegisterAsync(CreateUserRequest request)
        {
            var existingUser = await _userRepo.GetByEmailAsync(request.Email);
            if (existingUser != null)
            {
                throw new BusinessException("Email này đã được sử dụng trong hệ thống.");
            }

            return await _userRepo.CreateAsync(new User
            {
                Username = request.Name,
                Email = request.Email,
                PasswordHash = request.Password,
                CreatedAt = DateTime.UtcNow
            });
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _userRepo.GetByEmailAsync(email);
        }

        public async Task<User> GetProfileAsync(long userId)
        {
            var user = await _userRepo.GetByIdAsync(userId);
            if (user == null)
            {
                throw new BusinessException("Người dùng không tồn tại.");
            }
            return user;
        }

        public async Task ChangePasswordAsync(long userId, string newPasswordHash)
        {
            await _userRepo.UpdatePasswordAsync(userId, newPasswordHash);
        }
    }
}