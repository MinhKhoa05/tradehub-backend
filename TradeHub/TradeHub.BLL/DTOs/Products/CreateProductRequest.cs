using System.ComponentModel.DataAnnotations;

namespace TradeHub.BLL.DTOs.Products
{
    public class CreateProductRequest
    {
        [Required(ErrorMessage = "Tên sản phẩm không được để trống")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Tên sản phẩm phải từ 2 -> 50 ký tự")]
        public string Name { get; set; }

        public string? Description { get; set; }

        [Required(ErrorMessage = "Giá sản phẩm không được để trống")]
        [Range(0, double.MaxValue, ErrorMessage = "Giá sản phẩm không được âm")]
        public double Price { get; set; }
    }
}
