namespace TradeHub.DAL.Queries
{
    public class CartItemQuery
    {
        private readonly DatabaseContext _database;

        public CartItemQuery(DatabaseContext database)
        {
            _database = database;
        }

        public async Task<List<CartDetailDTO>> GetCartDetailDTOsAsync(long userId)
        {
            var sql = @"
                    SELECT
                        c.product_id,
                        p.name AS product_name,
                        p.price,
                        p.stock,
                        p.seller_id,
                        c.quantity
                    FROM cart_items c
                        JOIN products p ON p.id = c.product_id
                    WHERE c.user_id = @UserId
                ";

            return await _database.QueryAsync<CartDetailDTO>(sql, new { UserId = userId });
        }
    }

    public class CartDetailDTO
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public int Price { get; set; }
        public int TotalPrice => Price * Quantity;
        public int Stock { get; set; }
        public int SellerId { get; set; }
        public int Quantity { get; set; }
    }
}
