namespace GameTopUp.DAL.DTOs
{
    /// <summary>
    /// Data Transfer Object ch?a th�ng tin chi ti?t c?a m?t m?c trong gi? h�ng.
    /// </summary>
    public class CartDetailDTO
    {
        public long ProductId { get; set; }
        public string ProductName { get; set; } = null!;
        public decimal Price { get; set; }

        /// <summary>
        /// Th�nh ti?n c?a m?t m?c h�ng.
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
