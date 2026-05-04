using GameTopUp.DAL.Entities;

namespace GameTopUp.DAL.Interfaces
{
    public interface IOrderRepository
    {
        Task<Order?> GetByIdAsync(long orderId);
        Task<Order?> GetByIdForUpdateAsync(long orderId);
        Task<List<Order>> GetByUserIdAsync(long userId, OrderStatus? status = null);
        Task<long> CreateAsync(Order order);
        Task<int> UpdateAsync(Order order);
        Task<int> UpdateStatusAsync(long orderId, OrderStatus newStatus);
    }
}
