using TradeHub.DAL.Entities;
using TradeHub.DAL.Repositories.Interfaces;

namespace TradeHub.DAL.Repositories
{
    public class OrderLogRepository : IOrderLogRepository
    {
        private readonly DatabaseContext _database;

        public OrderLogRepository(DatabaseContext database)
        {
            _database = database;
        }

        public async Task<List<OrderLog>> GetByOrderIdAsync(long orderId)
        {
            var sql = "SELECT * FROM order_logs WHERE order_id = @OrderId ORDER BY created_at DESC";
            return await _database.QueryAsync<OrderLog>(sql, new { OrderId = orderId });
        }

        public async Task<long> CreateAsync(OrderLog log)
        {
            return await _database.InsertAsync<OrderLog, long>(log);
        }
    }
}
