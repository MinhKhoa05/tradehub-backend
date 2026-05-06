namespace GameTopUp.BLL.DTOs.GamePackages
{
    public class UpdateGamePackageRequest
    {
        public string? Name { get; set; }
        public string? ImageUrl { get; set; }
        public decimal? SalePrice { get; set; }
        public decimal? OriginalPrice { get; set; }
        public decimal? ImportPrice { get; set; }
        public int? StockQuantity { get; set; }
        public bool? IsActive { get; set; }
    }
}
