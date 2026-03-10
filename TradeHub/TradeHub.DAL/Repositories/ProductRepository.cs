using System.Data.Common;
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
            var sql = @"INSERT INTO products (name, normalized_name, description, price, stock, seller_id)
                    VALUES (@Name, @NormalizedName, @Description, @Price, @Stock, @SellerId)";
            return await _databaseContext.ExecuteInsertAsync(sql, product);
        }

        public async Task<Product?> GetByIdAsync(int id)
        {
            var sql = "SELECT * FROM products WHERE id = @Id";
            return await _databaseContext.QuerySingleAsync<Product>(sql, new { Id = id });
        }

        public async Task<List<Product>> GetByIdsAsync(IEnumerable<int> ids)
        {
            if (ids == null || !ids.Any())
                return new List<Product>();
            
            var sql = "SELECT * FROM products WHERE id IN @Ids";
            return await _databaseContext.QueryListAsync<Product>(sql, new { Ids = ids });
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
                SET name = @Name,
                    normalized_name = @NormalizedName,
                    description = @Description
                WHERE id = @Id";

            return await _databaseContext.ExecuteAsync(sql, new
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
            return await _databaseContext.ExecuteAsync(sql, new { Price = price, Id = productId });
        }

        public async Task<int> IncreaseStockAsync(int productId, int quantity)
        {
            var sql = "UPDATE products SET stock = stock + @Quantity WHERE id = @Id";
            return await _databaseContext.ExecuteAsync(sql, new {  Quantity = quantity, Id = productId });
        }

        public async Task<int> DecreaseStockAsync(int productId, int quantity)
        {
            var sql = "UPDATE products SET stock = stock - @Quantity WHERE id = @Id AND stock >= @Quantity";
            return await _databaseContext.ExecuteAsync(sql, new { Quantity = quantity,Id = productId });
        }

        public async Task<List<Product>> SearchByNameAsync(string normalizedName, int page, int pageSize)
        {
            var sql = """
                SELECT *
                FROM Products
                WHERE normalized_name LIKE '%@NormalizedName%'
                ORDER BY Id
                OFFSET @Offset ROWS
                FETCH NEXT @PageSize ROWS ONLY
            """;

            return await _databaseContext.QueryListAsync<Product>(sql, new
            {
                NormalizedName = normalizedName,
                Offset = (page - 1) * pageSize,
                PageSize = pageSize
            });
        }
    }
}
