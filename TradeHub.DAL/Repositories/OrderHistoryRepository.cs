using TradeHub.DAL.Entities;
using TradeHub.DAL.Interfaces;

namespace TradeHub.DAL.Repositories
{
    /// <summary>
    /// Repository quản lý lịch sử thay đổi trạng thái của đơn hàng (Audit Log).
    /// </summary>
    public class OrderHistoryRepository : IOrderHistoryRepository
    {
        private readonly DatabaseContext _database;

        public OrderHistoryRepository(DatabaseContext database)
        {
            _database = database;
        }
        
        public async Task<List<OrderHistory>> GetByOrderIdAsync(long orderId)
        {
            // Sắp xếp theo thời gian mới nhất lên đầu để dễ dàng theo dõi tiến trình.
            var sql = "SELECT * FROM order_history WHERE order_id = @OrderId ORDER BY created_at DESC";
            
            return await _database.QueryAsync<OrderHistory>(sql, new 
            { 
                OrderId = orderId 
            });
        }

        public async Task<long> CreateAsync(OrderHistory history)
        {
            return await _database.InsertAsync<OrderHistory, long>(history);
        }
    }
}
