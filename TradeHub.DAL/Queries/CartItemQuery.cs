using TradeHub.DAL.DTOs;

namespace TradeHub.DAL.Queries
{
    /// <summary>
    /// Thực hiện các truy vấn phức tạp liên quan đến giỏ hàng, bao gồm việc JOIN nhiều bảng 
    /// để lấy thông tin chi tiết gói nạp (tên, giá).
    /// </summary>
    public class CartItemQuery
    {
        private readonly DatabaseContext _database;

        public CartItemQuery(DatabaseContext database)
        {
            _database = database;
        }

        public async Task<List<CartDetailDTO>> GetCartDetailDTOsAsync(long userId)
        {
            // JOIN với bảng game_packages để lấy thông tin giá và tên sản phẩm tại thời điểm truy vấn.
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

            return await _database.QueryAsync<CartDetailDTO>(sql, new 
            { 
                UserId = userId 
            });
        }
    }
}
