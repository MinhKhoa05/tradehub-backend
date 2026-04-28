using TradeHub.DAL.Entities;
using TradeHub.DAL.Repositories.Interfaces;

namespace TradeHub.DAL.Repositories
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
                            package_budget = @PackageBudget, is_active = @IsActive, updated_at = CURRENT_TIMESTAMP
                        WHERE id = @Id";
            
            return await _database.ExecuteAsync(sql, gamePackage);
        }

        /// <summary>
        /// Cập nhật ngân sách đã chi tiêu cho gói nạp.
        /// Việc tách riêng hàm này giúp đảm bảo hiệu năng khi chỉ cần cập nhật một trường dữ liệu duy nhất.
        /// </summary>
        public async Task<int> UpdateStockBudgetAsync(long id, decimal spentAmountToAdd)
        {
            var sql = "UPDATE game_packages SET spent_amount = spent_amount + @SpentAmountToAdd WHERE id = @Id";
            
            return await _database.ExecuteAsync(sql, new 
            { 
                Id = id, 
                SpentAmountToAdd = spentAmountToAdd 
            });
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
