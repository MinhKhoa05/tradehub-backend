using System.ComponentModel.DataAnnotations;

namespace GameTopUp.BLL.DTOs.Carts
{
    public class CartItemRequest
    {
        public int ProductId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "S? lu?ng ph?i l?n hon 0")]
        public int Quantity { get; set; }
    }
}
