using TradeHub.DAL.Entities;
using TradeHub.DAL.DTOs;

namespace TradeHub.DAL.Repositories
{
    public class ProductRepository
    {
        private readonly DatabaseContext _database;

        public ProductRepository(DatabaseContext database)
        {
            _database = database;
        }

        public async Task<long> CreateAsync(Product product)
        {
            return await _database.InsertAsync(product);
        }

        public async Task<Product?> GetByIdAsync(int id)
        {
            var sql = "SELECT * FROM products WHERE id = @Id";
            return await _database.SqlFirstAsync<Product>(sql, new { Id = id });
        }

        public async Task<List<Product>> GetAllAsync()
        {
            var sql = "SELECT * FROM products";
            return await _database.SqlQueryAsync<Product>(sql);
        }

        public async Task<List<Product>> GetBySellerAsync(long sellerId)
        {
            var sql = "SELECT * FROM products WHERE seller_id = @SellerId";
            return await _database.SqlQueryAsync<Product>(sql, new { SellerId = sellerId });
        }

        public async Task<int> UpdateAsync(int productId, Product product)
        {
            var sql = @"
                UPDATE Products
                SET name = @Name,
                    normalized_name = @NormalizedName,
                    description = @Description
                WHERE id = @Id";

            return await _database.SqlExecuteAsync(sql, new
            {
                Id = productId,
                product.Name,
                product.NormalizedName,
                product.Description,
            });
        }

        public async Task<int> UpdatePriceAsync(int productId, int price)
        {
            var sql = "UPDATE products SET price = @Price WHERE id = @Id";
            return await _database.SqlExecuteAsync(sql, new { Price = price, Id = productId });
        }

        public async Task<int> IncreaseStockAsync(int productId, int quantity)
        {
            var sql = "UPDATE products SET stock = stock + @Quantity WHERE id = @Id";
            return await _database.SqlExecuteAsync(sql, new {  Quantity = quantity, Id = productId });
        }

        public async Task<int> DecreaseStockAsync(int productId, int quantity)
        {
            var sql = "UPDATE products SET stock = stock - @Quantity WHERE id = @Id AND stock >= @Quantity";
            return await _database.SqlExecuteAsync(sql, new { Quantity = quantity,Id = productId });
        }

        public async Task<int> DecreaseStockRangeAsync(List<ProductStockUpdate> products)
        {
            var sql = "UPDATE products SET stock = stock - @Quantity WHERE id = @Id AND stock >= @Quantity";
            return await _database.SqlExecuteAsync(sql, products);
        }

        public async Task<List<Product>> SearchByNameAsync(string normalizedName, int page, int pageSize)
        {
            var sql = @"
                SELECT *
                FROM products
                WHERE normalized_name LIKE @SearchPattern
                ORDER BY Id
                LIMIT @PageSize OFFSET @Offset
            ";

            return await _database.SqlQueryAsync<Product>(sql, new
            {
                SearchPattern = $"%{normalizedName}%",
                Offset = (page - 1) * pageSize,
                PageSize = pageSize
            });
        }
    }
}
