using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TradeHub.DAL.Entities
{
    [Table("game_packages")]
    public class GamePackage
    {
        [Key]
        public long Id { get; set; }
        public string Name { get; set; } = null!;
        public string ImageUrl { get; set; } = string.Empty;
        public string NormalizedName { get; set; } = null!;
        public long GameId { get; set; }

        
        public decimal SalePrice { get; set; }
        public decimal OriginalPrice { get; set; }
        public decimal ImportPrice { get; set; }
        
        public decimal PackageBudget { get; set; }
        public decimal SpentAmount { get; set; }
        
        public bool IsActive { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [NotMapped]
        public int CurrentStock
        {
            get
            {
                if (ImportPrice <= 0) return 0;
                var remainingBudget = PackageBudget - SpentAmount;
                return remainingBudget > 0 ? (int)Math.Floor(remainingBudget / ImportPrice) : 0;
            }
        }
    }
}