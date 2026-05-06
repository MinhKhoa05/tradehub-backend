using GameTopUp.DAL.Entities;
using GameTopUp.DAL.Interfaces;

namespace GameTopUp.DAL.Repositories
{
    /// <summary>
    /// Repository quản lý thông tin các gói nạp trong từng Game.
    /// </summary>
    public class GamePackageRepository : IGamePackageRepository
    {
        private readonly DatabaseContext _database;

        public GamePackageRepository(DatabaseContext database)
        {
            _database = database;
        }

        public async Task<GamePackage?> GetByIdAsync(long id)
        {
            var sql = "SELECT * FROM game_packages WHERE id = @Id";
            
            return await _database.QueryFirstAsync<GamePackage>(sql, new 
            { 
                Id = id 
            });
        }

        public async Task<List<GamePackage>> GetAllAsync()
        {
            var sql = "SELECT * FROM game_packages";
            
            return await _database.QueryAsync<GamePackage>(sql);
        }

        public async Task<List<GamePackage>> GetByGameIdAsync(long gameId)
        {
            // Chỉ lấy các gói nạp đang ở trạng thái hoạt động để hiển thị cho khách hàng.
            var sql = "SELECT * FROM game_packages WHERE game_id = @GameId AND is_active = 1";
            
            return await _database.QueryAsync<GamePackage>(sql, new 
            { 
                GameId = gameId 
            });
        }

        public async Task<long> CreateAsync(GamePackage gamePackage)
        {
            return await _database.InsertAsync<GamePackage, long>(gamePackage);
        }

        public async Task<int> UpdateAsync(GamePackage gamePackage)
        {
            var sql = @"UPDATE game_packages 
                        SET name = @Name, image_url = @ImageUrl, normalized_name = @NormalizedName, 
                            sale_price = @SalePrice, original_price = @OriginalPrice, import_price = @ImportPrice, 
                            stock_quantity = @StockQuantity, is_active = @IsActive, updated_at = CURRENT_TIMESTAMP
                        WHERE id = @Id";
            
            return await _database.ExecuteAsync(sql, gamePackage);
        }

        public async Task<int> IncreaseStockAsync(long id, int quantity)
        {
            var sql = @"UPDATE game_packages 
                        SET stock_quantity = stock_quantity + @Quantity, updated_at = CURRENT_TIMESTAMP
                        WHERE id = @Id";
            
            return await _database.ExecuteAsync(sql, new { Id = id, Quantity = quantity });
        }

        public async Task<int> DecreaseStockAsync(long id, int quantity)
        {
            var sql = @"UPDATE game_packages 
                        SET stock_quantity = stock_quantity - @Quantity, updated_at = CURRENT_TIMESTAMP
                        WHERE id = @Id AND stock_quantity >= @Quantity";
            
            return await _database.ExecuteAsync(sql, new { Id = id, Quantity = quantity });
        }


        public async Task<int> DeleteAsync(long id)
        {
            var sql = "DELETE FROM game_packages WHERE id = @Id";
            
            return await _database.ExecuteAsync(sql, new 
            { 
                Id = id 
            });
        }
    }
}
