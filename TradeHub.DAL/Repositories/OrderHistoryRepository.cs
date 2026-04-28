using TradeHub.DAL.Entities;
using TradeHub.DAL.Repositories.Interfaces;

namespace TradeHub.DAL.Repositories
{
    public class OrderHistoryRepository : IOrderHistoryRepository
    {
        private readonly DatabaseContext _database;

        public OrderHistoryRepository(DatabaseContext database)
        {
            _database = database;
        }
        
        public async Task<List<OrderHistory>> GetByOrderIdAsync(long orderId)
        {
            var sql = "SELECT * FROM order_history WHERE order_id = @OrderId ORDER BY created_at DESC";
            return await _database.QueryAsync<OrderHistory>(sql, new { OrderId = orderId });
        }

        public async Task<long> CreateAsync(OrderHistory history)
        {
            return await _database.InsertAsync<OrderHistory, long>(history);
        }
    }
}
