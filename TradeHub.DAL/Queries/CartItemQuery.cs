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
                        c.game_package_id AS ProductId,
                        p.name AS ProductName,
                        p.sale_price AS Price,
                        c.quantity
                    FROM cart_items c
                    JOIN game_packages p ON p.id = c.game_package_id
                    WHERE c.user_id = @UserId
                ";

            return await _database.QueryAsync<CartDetailDTO>(sql, new { UserId = userId });
        }
    }

    public class CartDetailDTO
    {
        public long ProductId { get; set; }
        public string ProductName { get; set; } = null!;
        public decimal Price { get; set; }
        public decimal TotalPrice => Price * Quantity;
        public int Quantity { get; set; }
    }
}
