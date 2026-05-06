using System.ComponentModel.DataAnnotations;

namespace GameTopUp.BLL.DTOs.Orders
{
    public class PlaceOrderRequestDTO
    {
        [Required]
        public long GamePackageId { get; set; }

        [Range(1, 1000)]
        public int Quantity { get; set; } = 1;

        [Required]
        public string GameAccountInfo { get; set; } = string.Empty;
    }
}
