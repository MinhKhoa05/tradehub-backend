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
                Name = user.Username, // Map Username từ Entity sang Name trong DTO
                Email = user.Email,
            };
        }
    }
}
