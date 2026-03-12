using TradeHub.DAL.Entities;

namespace TradeHub.DAL.Repositories
{
    public class UserRepository
    {
        private readonly DatabaseContext _database;

        public UserRepository(DatabaseContext database)
        {
            _database = database;
        }

        public async Task<User?> GetByIdAsync(int userId)
        {
            string sql = "SELECT * FROM users WHERE id = @UserId";
            return await _database.QuerySingleAsync<User>(sql, new { UserId = userId });
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            string sql = "SELECT * FROM users WHERE email = @Email";
            return await _database.QuerySingleAsync<User>(sql, new { Email = email });
        }

        public async Task<int> CreateAsync(User user)
        {
            string sql = @"
                INSERT INTO users (name, email, password_hash, balance)
                VALUES (@Name, @Email, @PasswordHash, @Balance)";

            return await _database.ExecuteInsertAsync(sql, user);
        }

        public async Task<int> UpdatePasswordAsync(int userId, string newPasswordHash)
        {
            string sql = "UPDATE users SET password_hash = @PasswordHash WHERE id = @UserId";
            return await _database.ExecuteAsync(sql, new { PasswordHash = newPasswordHash, UserId = userId });
        }

        public async Task<int> UpdateAsync(User user)
        {
            string sql = @"UPDATE users SET name = @Name, email = @Email
                    WHERE id = @Id";
            return await _database.ExecuteAsync(sql, user);
        }
    }
}