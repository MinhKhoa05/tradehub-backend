using TradeHub.BLL.DTOs.Users;
using TradeHub.BLL.Exceptions;
using TradeHub.DAL.Entities;
using TradeHub.DAL.Repositories;

namespace TradeHub.BLL.Services
{
    public class UserService
    {
        private readonly UserRepository _userRepository;
        
        public UserService(UserRepository userRepository)
        {
            _userRepository = userRepository;
        }
        
        public async Task CreateUserAsync(CreateUserRequest request)
        {
            // Kiểm tra email đã được đăng ký chưa?
            var existingUser = await _userRepository.GetByEmailAsync(request.Email);
            if (existingUser != null)
            {
                throw new BusinessException("Email đã được sử dụng");
            }

            // Thêm user mới
            var user = new User
            {
                Name = request.Name,
                Email = request.Email,
                PasswordHash = request.Password,
                Balance = 0
            };

            await _userRepository.CreateAsync(user);
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _userRepository.GetByEmailAsync(email);
        }

        public async Task UpdatePasswordAsync(int userId, string newPasswordHash)
        {
            await _userRepository.UpdatePasswordAsync(userId, newPasswordHash);
        }

        public async Task<User> GetUserByIdOrThrowAsync(int userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                throw new NotFoundException("User", "id", userId);
            }

            return user;
        }
    }
}
