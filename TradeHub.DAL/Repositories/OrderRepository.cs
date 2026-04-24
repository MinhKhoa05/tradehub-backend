using TradeHub.DAL.Entities;
using TradeHub.DAL.Repositories.Interfaces;

namespace TradeHub.DAL.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly DatabaseContext _database;

        public OrderRepository(DatabaseContext database)
        {
            _database = database;
        }

        public async Task<Order?> GetByIdAsync(long orderId)
        {
            var sql = "SELECT * FROM orders WHERE id = @Id";
            return await _database.QueryFirstAsync<Order>(sql, new { Id = orderId });
        }

        public async Task<List<Order>> GetByUserIdAsync(long userId, OrderStatus? status = null)
        {
            var sql = @"SELECT * FROM orders
                        WHERE user_id = @UserId
                        AND (@Status IS NULL OR status = @Status)
                        ORDER BY created_at DESC";
            return await _database.QueryAsync<Order>(sql, new { UserId = userId, Status = status });
        }

        public async Task<bool> IsOrderBelongsToUserAsync(long userId, long orderId)
        {
            var sql = @"
                SELECT EXISTS (
                    SELECT 1
                    FROM orders
                    WHERE id = @OrderId AND user_id = @UserId
                );
            ";

            return await _database.ScalarAsync<bool>(sql, new { UserId = userId, OrderId = orderId });
        }

        public async Task<long> CreateAsync(Order order)
        {
            return await _database.InsertAsync<Order, long>(order);
        }

        public async Task<int> UpdateStatusAsync(long orderId, OrderStatus newStatus)
        {
            var sql = "UPDATE orders SET status = @Status, updated_at = CURRENT_TIMESTAMP WHERE id = @Id";
            return await _database.ExecuteAsync(sql, new { Id = orderId, Status = newStatus });
        }
    }
}