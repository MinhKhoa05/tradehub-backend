using TradeHub.DAL.Entities;

namespace TradeHub.DAL.Repositories
{
    /// <summary>
    /// Repository quản lý thông tin người dùng và số dư ví.
    /// Đây là một trong những Repository quan trọng nhất liên quan đến tài chính.
    /// </summary>
    public class UserRepository
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
            string sql = @"UPDATE users 
                           SET username = @Username, email = @Email, role = @Role, is_active = @IsActive
                           WHERE id = @Id";
            
            return await _database.ExecuteAsync(sql, user);
        }

        public async Task<int> IncreaseBalanceAsync(long userId, decimal amount)
        {
            var sql = "UPDATE users SET balance = balance + @Amount WHERE id = @UserId";
            
            return await _database.ExecuteAsync(sql, new 
            { 
                UserId = userId, 
                Amount = amount 
            });
        }

        /// <summary>
        /// Trừ tiền trong ví người dùng. 
        /// Ràng buộc balance >= @Amount ngay trong câu lệnh SQL để đảm bảo tính nhất quán 
        /// và tránh lỗi Race Condition khi có nhiều yêu cầu trừ tiền đồng thời.
        /// </summary>
        public async Task<int> DecreaseBalanceAsync(long userId, decimal amount)
        {
            var sql = "UPDATE users SET balance = balance - @Amount WHERE id = @UserId AND balance >= @Amount";
            
            return await _database.ExecuteAsync(sql, new 
            { 
                UserId = userId, 
                Amount = amount 
            });
        }
    }
}