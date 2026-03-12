using TradeHub.BLL.DTOs.Carts;
using TradeHub.DAL.Entities;

namespace TradeHub.BLL.Mappings
{
    public static class CartMapping
    {
        public static CartItemDTO ToCartItemDTO(this CartItem cartItem, Product? product)
        {
            return new CartItemDTO
            {
                Id = cartItem.Id,
                ProductId = cartItem.ProductId,
                Quantity = cartItem.Quantity,

                Product = product == null ? null : new CartProductDTO
                {
                    ProductId = product.Id,
                    Name = product.Name,
                    Price = product.Price
                }
            };
        }
    }
}
