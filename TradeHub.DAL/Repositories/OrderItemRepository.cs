using TradeHub.DAL.Entities;

namespace TradeHub.DAL.Repositories
{
    public class OrderItemRepository
    {
        private readonly DatabaseContext _database;

        public OrderItemRepository(DatabaseContext database)
        {
            _database = database;
        }

        public async Task<List<OrderItem>> GetByOrderIdAsync(int orderId)
        {
            var sql = "SELECT * FROM order_items WHERE order_id = @OrderId";
            return await _database.SqlQueryAsync<OrderItem>(sql, new { OrderId = orderId });
        }

        public async Task<int> CreateRangeAsync(IEnumerable<OrderItem> items)
        {
            var sql = @"INSERT INTO order_items (order_id, product_id, unit_price, quantity)
                        VALUES (@OrderId, @ProductId, @UnitPrice, @Quantity)"; 
            return await _database.SqlExecuteAsync(sql, items);
        }
    }
}
