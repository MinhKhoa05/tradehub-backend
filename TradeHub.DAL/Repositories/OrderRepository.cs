using TradeHub.DAL.Entities;

namespace TradeHub.DAL.Repositories
{
    public class OrderRepository
    {
        private readonly DatabaseContext _database;

        public OrderRepository(DatabaseContext database)
        {
            _database = database;
        }

        public async Task<Order?> GetByIdAsync(int orderId)
        {
            var sql = "SELECT * FROM orders WHERE id = @Id";
            return await _database.QuerySingleAsync<Order>(sql, new { Id = orderId });
        }

        public async  Task<List<Order>> GetAllByUserId(int userId, OrderStatus? status = null)
        {
            var sql = @"SELECT * FROM orders
                        WHERE (seller_id = @UserId OR buyer_id = @UserId)
                        AND (@Status IS NULL OR status = @Status)
                        ORDER BY created_at DESC";
            return await _database.QueryListAsync<Order>(sql, new { UserId = userId, Status = status });
        }

        public async Task<List<Order>> GetSellerOrdersAsync(int userId, OrderStatus? status = null)
        {
            var sql = @"SELECT * FROM orders
                        WHERE seller_id = @UserId
                        AND (@Status IS NULL OR status = @Status)
                        ORDER BY created_at DESC";
            return await _database.QueryListAsync<Order>(sql, new { UserId = userId, Status = status });
        }

        public async Task<bool> IsOrderBelongsToUserAsync(int userId, int orderId)
        {
            var sql = @"
                SELECT 1
                FROM orders
                WHERE id = @OrderId
                  AND (buyer_id = @UserId OR seller_id = @UserId)
                LIMIT 1"; // chỉ cần 1 dòng

            var result = await _database.ExecuteScalarAsync<int?>(sql, new { UserId = userId, OrderId = orderId });

            return result.HasValue;
        }

        public async Task<List<Order>> GetBuyerOrdersAsync(int userId, OrderStatus? status = null)
        {
            var sql = @"SELECT * FROM orders
                        WHERE buyer_id = @UserId
                        AND (@Status IS NULL OR status = @Status)
                        ORDER BY created_at DESC";
            return await _database.QueryListAsync<Order>(sql, new { UserId = userId, Status = status });
        }

        public async Task<int> CreateAsync(Order order)
        {
            var sql = @"INSERT INTO orders (buyer_id, seller_id, total_amount, payment_method, status)
                        VALUES (@BuyerId, @SellerId, @TotalAmount, @PaymentMethod, @Status)";
            return await _database.ExecuteInsertAsync(sql, order);
        }

        public async Task<int> UpdateStatusAsync(int orderId, OrderStatus newStatus)
        {
            var sql = "UPDATE orders SET status = @Status, updated_at = CURRENT_TIMESTAMP WHERE id = @Id";
            return await _database.ExecuteAsync(sql, new { Id = orderId, Status = newStatus });
        }
    }
}
