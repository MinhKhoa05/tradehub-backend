using TradeHub.DAL.Entities;

namespace TradeHub.DAL.Repositories.Interfaces
{
    public interface IOrderLogRepository
    {
        Task<List<OrderLog>> GetByOrderIdAsync(long orderId);
        Task<long> CreateAsync(OrderLog log);
    }
}
