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

        public async Task<User?> GetByIdAsync(long userId)
        {
            string sql = "SELECT * FROM users WHERE id = @UserId";
            return await _database.SqlFirstAsync<User>(sql, new { UserId = userId });
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            string sql = "SELECT * FROM users WHERE email = @Email";
            return await _database.SqlFirstAsync<User>(sql, new { Email = email });
        }

        public async Task<long> CreateAsync(User user)
        {
            return await _database.InsertAsync(user);
        }

        public async Task<int> UpdatePasswordAsync(long userId, string newPasswordHash)
        {
            string sql = "UPDATE users SET password_hash = @PasswordHash WHERE id = @UserId";
            return await _database.SqlExecuteAsync(sql, new { PasswordHash = newPasswordHash, UserId = userId });
        }

        public async Task<int> UpdateAsync(User user)
        {
            string sql = @"UPDATE users SET name = @Name, email = @Email
                    WHERE id = @Id";
            return await _database.SqlExecuteAsync(sql, user);
        }
    }
}