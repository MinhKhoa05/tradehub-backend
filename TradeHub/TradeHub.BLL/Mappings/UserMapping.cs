using TradeHub.BLL.DTOs.Users;
using TradeHub.DAL.Entities;

namespace TradeHub.BLL.Mappings
{
    public static class UserMapping
    {
        public static UserResponse ToResponse(this User user)
        {
            return new UserResponse
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
            };
        }
    }
}
