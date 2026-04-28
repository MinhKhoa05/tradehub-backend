using TradeHub.BLL.DTOs.Carts;
using TradeHub.DAL.Entities;

namespace TradeHub.BLL.Mappings
{
    public static class CartMapping
    {
        public static CartItemDTO ToCartItemDTO(this CartItem cartItem, GamePackage? product)
        {
            return new CartItemDTO
            {
                Id = cartItem.Id,
                ProductId = cartItem.GamePackageId,
                Quantity = cartItem.Quantity,

                Product = product == null ? null : new CartProductDTO
                {
                    ProductId = product.Id,
                    Name = product.Name,
                    Price = (int)product.SalePrice // Ép kiểu nếu DTO yêu cầu int, hoặc sửa DTO sau
                }
            };
        }
    }
}
