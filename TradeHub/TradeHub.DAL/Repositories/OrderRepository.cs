using TradeHub.DAL.Entities;

namespace TradeHub.DAL.Repositories
{
    public class OrderRepository
    {
        private readonly DatabaseContext _databaseContext;

        public OrderRepository(DatabaseContext databaseContext)
        {
            _databaseContext = databaseContext;
        }

        public async Task<Order?> GetByIdAsync(int orderId)
        {
            var sql = "SELECT * FROM orders WHERE id = @Id";
            return await _databaseContext.QuerySingleAsync<Order>(sql, new { Id = orderId });
        }

        public async Task<List<Order>> GetSellerOrdersAsync(int sellerId, OrderStatus? status)
        {
            var sql = @"SELECT * FROM orders
                        WHERE seller_id = @SellerId
                        AND (@Status IS NULL OR status = @Status)
                        ORDER BY created_at DESC";
            return await _databaseContext.QueryListAsync<Order>(sql, new { SellerId = sellerId, Status = status });
        }

        public async Task<List<Order>> GetBuyerOrdersAsync(int buyerId, OrderStatus? status)
        {
            var sql = @"SELECT * FROM orders
                        WHERE buyer_id = @BuyerId
                        AND (@Status IS NULL OR status = @Status)
                        ORDER BY created_at DESC";
            return await _databaseContext.QueryListAsync<Order>(sql, new { BuyerId = buyerId, Status = status });
        }

        public async Task<int> CreateAsync(Order order)
        {
            var sql = @"INSERT INTO orders (buyer_id, seller_id, total_amount, payment_method, status)
                        VALUES (@BuyerId, @SellerId, @TotalAmount, @PaymentMethod, @Status)";
            return await _databaseContext.ExecuteInsertAsync(sql, order);
        }

        public async Task<int> UpdateStatusAsync(int orderId, OrderStatus newStatus)
        {
            var sql = "UPDATE orders SET status = @Status, updated_at = CURRENT_TIMESTAMP WHERE id = @Id";
            return await _databaseContext.ExecuteAsync(sql, new { Id = orderId, Status = newStatus });
        }
    }
}
