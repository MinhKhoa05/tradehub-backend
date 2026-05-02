using Mapster;
using TradeHub.BLL.Common;
using TradeHub.BLL.DTOs.Users;
using TradeHub.BLL.Exceptions;
using TradeHub.DAL.Entities;
using TradeHub.DAL.Repositories.Interfaces;

namespace TradeHub.BLL.Services
{
    public class UserService
    {
        private readonly IUserRepository _userRepo;

        public UserService(IUserRepository userRepo)
        {
            _userRepo = userRepo;
        }

        public async Task<IEnumerable<UserResponseDTO>> GetAllAsync(int page, int pageSize)
        {
            var users = await _userRepo.GetAllAsync(page, pageSize);
            return users.Adapt<IEnumerable<UserResponseDTO>>();
        }

        public async Task<UserResponseDTO> GetByIdAsync(long id)
        {
            var user = await _userRepo.GetByIdAsync(id) ?? throw new NotFoundException("Người dùng không tồn tại.");
            return user.Adapt<UserResponseDTO>();
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

        public async Task UpdateProfileAsync(long id, UpdateUserRequest request)
        {
            var user = await _userRepo.GetByIdAsync(id) ?? throw new NotFoundException("Người dùng không tồn tại.");
            request.Adapt(user);
            await _userRepo.UpdateAsync(user);
        }

        public async Task DeleteAsync(long id)
        {
            var user = await _userRepo.GetByIdAsync(id) ?? throw new NotFoundException("Người dùng không tồn tại.");
            await _userRepo.DeleteAsync(id);
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _userRepo.GetByEmailAsync(email);
        }

        public async Task<User> GetProfileAsync(long userId)
        {
            var user = await _userRepo.GetByIdAsync(userId) ?? throw new NotFoundException("Người dùng không tồn tại.");
            return user;
        }

        public async Task ChangePasswordAsync(long userId, string newPasswordHash)
        {
            await _userRepo.UpdatePasswordAsync(userId, newPasswordHash);
        }
    }
}