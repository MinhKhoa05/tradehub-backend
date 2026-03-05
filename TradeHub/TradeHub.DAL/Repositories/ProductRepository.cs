using TradeHub.DAL.Entities;

namespace TradeHub.DAL.Repositories
{
    public class ProductRepository
    {
        private readonly DatabaseContext _databaseContext;

        public ProductRepository(DatabaseContext databaseContext)
        {
            _databaseContext = databaseContext;
        }

        public async Task<int> CreateAsync(Product product)
        {
            var sql = @"INSERT INTO products (name, description, price, stock, seller_id)
                    VALUES (@Name, @Decription, @Price, @Stock, @SellerId);
                    SELECT LAST_INSERT_ID();";
            return await _databaseContext.ExecuteScalarAsync<int>(sql);
        }

        public async Task<Product?> GetByIdAsync(int id)
        {
            var sql = "SELECT * FROM products WHERE id = @Id";
            return await _databaseContext.QuerySingleAsync<Product>(sql, new { Id = id });
        }

        public async Task<List<Product>> GetAllAsync()
        {
            var sql = "SELECT * FROM products";
            return await _databaseContext.QueryListAsync<Product>(sql);
        }

        public async Task<List<Product>> GetBySellerAsync(int sellerId)
        {
            var sql = "SELECT * FROM products WHERE seller_id = @SellerId";
            return await _databaseContext.QueryListAsync<Product>(sql, new { SellerId = sellerId });
        }

        public async Task<int> UpdateAsync(int productId, Product product)
        {
            var sql = @"
                UPDATE Products
                SET Name = @Name,
                    Description = @Description,
                WHERE id = @Id";

            return await _databaseContext.ExecuteAsync(sql, new
            {
                Id = productId,
                product.Name,
                product.Description,
            });
        }

        public async Task<int> UpdatePriceBySellerAsync(int productId, double price, int sellerId)
        {
            var sql = "UPDATE products SET price = @Price WHERE id = @Id AND seller_id = @SellerId";
            return await _databaseContext.ExecuteAsync(sql, new { Price = price, Id = productId, SellerId = sellerId });
        }

        public async Task<int> IncreaseStockBySellerAsync(int productId, int quantity, int sellerId)
        {
            var sql = "UPDATE products SET stock = stock + @Quantity WHERE id = @Id AND seller_id = @SellerId";
            return await _databaseContext.ExecuteAsync(sql, new {  Quantity = quantity, Id = productId, SellerId = sellerId });
        }

        public async Task<int> DecreaseStockBySellerAsync(int productId, int quantity, int sellerId)
        {
            var sql = "UPDATE products SET stock = stock - @Quantity WHERE id = @Id AND stock >= @Quantity AND seller_id = @SellerId";
            return await _databaseContext.ExecuteAsync(sql, new { Quantity = quantity,Id = productId, SellerId = sellerId });
        }
    }
}
