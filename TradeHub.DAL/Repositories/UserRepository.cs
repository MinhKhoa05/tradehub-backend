using TradeHub.DAL.Entities;
using TradeHub.DAL.Interfaces;

namespace TradeHub.DAL.Repositories
{
    /// <summary>
    /// Repository quản lý thông tin người dùng và số dư ví.
    /// Đây là một trong những Repository quan trọng nhất liên quan đến tài chính.
    /// </summary>
    public class UserRepository : IUserRepository
    {
        private readonly DatabaseContext _database;

        public UserRepository(DatabaseContext database)
        {
            _database = database;
        }

        public async Task<User?> GetByIdAsync(long userId)
        {
            string sql = "SELECT * FROM users WHERE id = @UserId";
            
            return await _database.QueryFirstAsync<User>(sql, new 
            { 
                UserId = userId 
            });
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            string sql = "SELECT * FROM users WHERE email = @Email";
            
            return await _database.QueryFirstAsync<User>(sql, new 
            { 
                Email = email 
            });
        }

        public async Task<long> CreateAsync(User user)
        {
            return await _database.InsertAsync<User, long>(user);
        }

        public async Task<int> UpdatePasswordAsync(long userId, string newPasswordHash)
        {
            string sql = "UPDATE users SET password_hash = @PasswordHash WHERE id = @UserId";
            
            return await _database.ExecuteAsync(sql, new 
            { 
                PasswordHash = newPasswordHash, 
                UserId = userId 
            });
        }

        public async Task<int> UpdateAsync(User user)
        {
            user.UpdatedAt = DateTime.UtcNow;
            string sql = @"UPDATE users 
                           SET username = @Username, email = @Email, role = @Role, is_active = @IsActive, updated_at = @UpdatedAt
                           WHERE id = @Id";
            
            return await _database.ExecuteAsync(sql, user);
        }

        public async Task<IEnumerable<User>> GetAllAsync(int page, int pageSize)
        {
            int offset = (page - 1) * pageSize;
            string sql = "SELECT * FROM users ORDER BY created_at DESC LIMIT @Limit OFFSET @Offset";
            
            return await _database.QueryAsync<User>(sql, new 
            { 
                Limit = pageSize, 
                Offset = offset 
            });
        }

        public async Task<int> DeleteAsync(long userId)
        {
            string sql = "UPDATE users SET is_active = 0 WHERE id = @UserId";
            
            return await _database.ExecuteAsync(sql, new 
            { 
                UserId = userId 
            });
        }

    }
}
