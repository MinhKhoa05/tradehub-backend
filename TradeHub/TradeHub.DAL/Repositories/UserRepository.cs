using TradeHub.DAL.Entities;

namespace TradeHub.DAL.Repositories
{
    public class UserRepository
    {
        private readonly DatabaseContext _dbContext;

        public UserRepository(DatabaseContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<User?> GetByIdAsync(int userId)
        {
            string sql = "SELECT * FROM users WHERE id = @UserId";
            return await _dbContext.QuerySingleAsync<User>(sql, new { UserId = userId });
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            string sql = "SELECT * FROM users WHERE email = @Email";
            return await _dbContext.QuerySingleAsync<User>(sql, new { Email = email });
        }

        public async Task<int> CreateAsync(User user)
        {
            string sql = @"
                INSERT INTO users (name, email, password_hash, balance)
                VALUES (@Name, @Email, @PasswordHash, @Balance)";

            return await _dbContext.ExecuteInsertAsync(sql, user);
        }

        public async Task<int> UpdatePasswordAsync(int userId, string newPasswordHash)
        {
            string sql = "UPDATE users SET password_hash = @PasswordHash WHERE id = @UserId";
            return await _dbContext.ExecuteAsync(sql, new { PasswordHash = newPasswordHash, UserId = userId });
        }

        public async Task<int> UpdateAsync(User user)
        {
            string sql = @"UPDATE users SET name = @Name, email = @Email
                    WHERE id = @Id";
            return await _dbContext.ExecuteAsync(sql, user);
        }
    }
}