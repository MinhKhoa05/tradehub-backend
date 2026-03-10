using TradeHub.DAL.Entities;

namespace TradeHub.DAL.Repositories
{
    public class OrderHistoryRepository
    {
        private readonly DatabaseContext _databaseContext;

        public OrderHistoryRepository(DatabaseContext databaseContext)
        {
            _databaseContext = databaseContext;
        }

        public async Task<List<OrderHistory>> GetByOrderIdAsync(int orderId)
        {
            var sql = "SELECT * FROM order_history WHERE order_id = @OrderId";
            return await _databaseContext.QueryListAsync<OrderHistory>(sql, new { OrderId = orderId });
        }

        public async Task<int> CreateAsync(OrderHistory history)
        {
            var sql = @"INSERT INTO order_history (order_id, from_status, to_status, changed_by, actor_type, note)
                        VALUES (@OrderId, @FromStatus, @ToStatus, @ChangedBy, @ActorType, @Note)";
            return await _databaseContext.ExecuteInsertAsync(sql, history);
        }
    }
}
