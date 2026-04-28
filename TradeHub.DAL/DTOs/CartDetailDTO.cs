namespace TradeHub.DAL.DTOs
{
    /// <summary>
    /// Data Transfer Object chứa thông tin chi tiết của một mục trong giỏ hàng.
    /// </summary>
    public class CartDetailDTO
    {
        public long ProductId { get; set; }
        public string ProductName { get; set; } = null!;
        public decimal Price { get; set; }

        /// <summary>
        /// Thành tiền của một mục hàng.
        /// </summary>
        public decimal TotalPrice
        {
            get
            {
                return Price * Quantity;
            }
        }

        public int Quantity { get; set; }
    }
}
