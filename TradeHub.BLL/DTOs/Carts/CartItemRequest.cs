using System.ComponentModel.DataAnnotations;

namespace TradeHub.BLL.DTOs.Carts
{
    public class CartItemRequest
    {
        public int ProductId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Số lượng phải lớn hơn 0")]
        public int Quantity { get; set; }
    }
}
