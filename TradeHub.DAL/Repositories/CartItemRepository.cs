using TradeHub.DAL.Entities;

namespace TradeHub.DAL.Repositories
{
    public class CartItemRepository
    {
        private readonly DatabaseContext _database;

        public CartItemRepository(DatabaseContext database)
        {
            _database = database;
        }

        public async Task<int> CreateAsync(CartItem cartItem)
        {
            var sql = @"INSERT INTO cart_items (user_id, product_id, quantity)
                        VALUES (@UserId, @ProductId, @Quantity)";
            return await _database.ExecuteInsertAsync(sql, cartItem);
        }

        public async Task<int> UpdateQuantityAsync(int userId, int productId, int quantity)
        {
            var sql = @"UPDATE cart_items
                    SET quantity = @Quantity
                    WHERE user_id = @UserId AND product_id = @ProductId";
            return await _database.ExecuteAsync(sql, new { UserId = userId, ProductId = productId, Quantity = quantity });
        }

        public async Task<int> DeleteAsync(int userId, int productId)
        {
            var sql = "DELETE FROM cart_items WHERE user_id = @UserId AND product_id = @ProductId";
            return await _database.ExecuteAsync(sql, new { UserId = userId, ProductId = productId });
        }

        public async Task<int> DeleteByUserIdAsync(int userId)
        {
            var sql = "DELETE FROM cart_items WHERE user_id = @UserId";
            return await _database.ExecuteAsync(sql, new { UserId = userId });
        }

        public async Task<List<CartItem>> GetByUserIdAsync(int userId)
        {
            var sql = "SELECT * FROM cart_items WHERE user_id = @UserId";
            return await _database.QueryListAsync<CartItem>(sql, new { UserId = userId });
        }

        public async Task<CartItem?> GetByUserProductAsync(int userId, int productId)
        {
            var sql = "SELECT * FROM cart_items WHERE user_id = @UserId AND product_id = @ProductId";
            return await _database.QuerySingleAsync<CartItem>(sql, new { UserId = userId, ProductId = productId });
        }

        public async Task<int> GetTotalQuantityAsync(int userId)
        {
            var sql = "SELECT COALESCE(SUM(quantity), 0) FROM cart_items WHERE user_id = @UserId";
            return await _database.ExecuteScalarAsync<int>(sql, new { UserId = userId });
        }
    }
}
