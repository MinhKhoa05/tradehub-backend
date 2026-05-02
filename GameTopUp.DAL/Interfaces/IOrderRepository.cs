using GameTopUp.DAL.Entities;

namespace GameTopUp.DAL.Interfaces
{
    public interface IOrderRepository
    {
        Task<Order?> GetByIdAsync(long orderId);
        Task<List<Order>> GetByUserIdAsync(long userId, OrderStatus? status = null);
        Task<bool> IsOrderBelongsToUserAsync(long userId, long orderId);
        Task<long> CreateAsync(Order order);
        Task<int> UpdateStatusAsync(long orderId, OrderStatus newStatus);
    }
}
