using TradeHub.DAL.Entities;

namespace TradeHub.DAL.Interfaces
{
    public interface IOrderHistoryRepository
    {
        Task<List<OrderHistory>> GetByOrderIdAsync(long orderId);
        Task<long> CreateAsync(OrderHistory history);
    }
}
