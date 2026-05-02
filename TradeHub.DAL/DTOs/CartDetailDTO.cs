namespace TradeHub.DAL.DTOs
{
    /// <summary>
    /// Data Transfer Object ch?a thông tin chi ti?t c?a m?t m?c trong gi? hàng.
    /// </summary>
    public class CartDetailDTO
    {
        public long ProductId { get; set; }
        public string ProductName { get; set; } = null!;
        public decimal Price { get; set; }

        /// <summary>
        /// Thành ti?n c?a m?t m?c hàng.
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
