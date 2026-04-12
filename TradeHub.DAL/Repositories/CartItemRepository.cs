using SqlKata.Execution;
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

        public async Task<long> CreateAsync(CartItem cartItem)
        {
            return await _database.InsertAsync(cartItem);
        }

        public async Task<int> UpdateQuantityAsync(long userId, long productId, int quantity)
        {
            var sql = @"UPDATE cart_items
                    SET quantity = @Quantity
                    WHERE user_id = @UserId AND product_id = @ProductId";
            return await _database.SqlExecuteAsync(sql, new { UserId = userId, ProductId = productId, Quantity = quantity });
        }

        public async Task<int> DeleteAsync(long userId, long productId)
        {
            var sql = "DELETE FROM cart_items WHERE user_id = @UserId AND product_id = @ProductId";
            return await _database.SqlExecuteAsync(sql, new { UserId = userId, ProductId = productId });
        }

        public async Task<int> DeleteByUserIdAsync(long userId)
        {
            var sql = "DELETE FROM cart_items WHERE user_id = @UserId";
            return await _database.SqlExecuteAsync(sql, new { UserId = userId });
        }

        public async Task<List<CartItem>> GetByUserIdAsync(long userId)
        {
            var sql = "SELECT * FROM cart_items WHERE user_id = @UserId";
            return await _database.SqlQueryAsync<CartItem>(sql, new { UserId = userId });
        }

        public async Task<CartItem?> GetByUserProductAsync(long userId, long productId)
        {
            var sql = "SELECT * FROM cart_items WHERE user_id = @UserId AND product_id = @ProductId";
            return await _database.SqlFirstAsync<CartItem>(sql, new { UserId = userId, ProductId = productId });
        }

        public async Task<int> GetTotalQuantityAsync(long userId)
        {
            var sql = "SELECT COALESCE(SUM(quantity), 0) FROM cart_items WHERE user_id = @UserId";
            return await _database.SqlScalarAsync<int>(sql, new { UserId = userId });
        }
    }
}