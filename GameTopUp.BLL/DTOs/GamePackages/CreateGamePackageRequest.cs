using System.ComponentModel.DataAnnotations;

namespace GameTopUp.BLL.DTOs.GamePackages
{
    public class CreateGamePackageRequest
    {
        [Required]
        public string Name { get; set; } = null!;
        public string ImageUrl { get; set; } = string.Empty;
        
        [Required]
        public long GameId { get; set; }
        
        [Range(0, double.MaxValue)]
        public decimal SalePrice { get; set; }
        
        [Range(0, double.MaxValue)]
        public decimal OriginalPrice { get; set; }
        
        [Range(0, double.MaxValue)]
        public decimal ImportPrice { get; set; }
        
        [Range(0, double.MaxValue)]
        public decimal PackageBudget { get; set; }
        
        public bool IsActive { get; set; } = true;
    }
}
