using GameTopUp.DAL.Entities;

namespace GameTopUp.DAL.Repositories
{
    /// <summary>
    /// Quản lý dữ liệu giỏ hàng của người dùng.
    /// Repository này tương tác trực tiếp với bảng cart_items.
    /// </summary>
    public class CartItemRepository
    {
        private readonly DatabaseContext _database;

        public CartItemRepository(DatabaseContext database)
        {
            _database = database;
        }

        public async Task<long> CreateAsync(CartItem cartItem)
        {
            return await _database.InsertAsync<CartItem, long>(cartItem);
        }

        public async Task<int> UpdateQuantityAsync(long userId, long gamePackageId, int quantity)
        {
            var sql = @"UPDATE cart_items
                        SET quantity = @Quantity
                        WHERE user_id = @UserId AND game_package_id = @GamePackageId";
            
            return await _database.ExecuteAsync(sql, new 
            { 
                UserId = userId, 
                GamePackageId = gamePackageId, 
                Quantity = quantity 
            });
        }

        public async Task<int> DeleteAsync(long userId, long gamePackageId)
        {
            var sql = "DELETE FROM cart_items WHERE user_id = @UserId AND game_package_id = @GamePackageId";
            
            return await _database.ExecuteAsync(sql, new 
            { 
                UserId = userId, 
                GamePackageId = gamePackageId 
            });
        }

        public async Task<int> DeleteByUserIdAsync(long userId)
        {
            var sql = "DELETE FROM cart_items WHERE user_id = @UserId";
            
            return await _database.ExecuteAsync(sql, new 
            { 
                UserId = userId 
            });
        }

        public async Task<List<CartItem>> GetByUserIdAsync(long userId)
        {
            var sql = "SELECT * FROM cart_items WHERE user_id = @UserId";
            
            return await _database.QueryAsync<CartItem>(sql, new 
            { 
                UserId = userId 
            });
        }

        public async Task<CartItem?> GetByUserPackageAsync(long userId, long gamePackageId)
        {
            var sql = "SELECT * FROM cart_items WHERE user_id = @UserId AND game_package_id = @GamePackageId";
            
            var items = await _database.QueryAsync<CartItem>(sql, new 
            { 
                UserId = userId, 
                GamePackageId = gamePackageId 
            });
            
            return items.FirstOrDefault();
        }

        public async Task<int> GetTotalQuantityAsync(long userId)
        {
            var sql = "SELECT COALESCE(SUM(quantity), 0) FROM cart_items WHERE user_id = @UserId";
            
            return await _database.ScalarAsync<int>(sql, new 
            { 
                UserId = userId 
            });
        }
    }
}
