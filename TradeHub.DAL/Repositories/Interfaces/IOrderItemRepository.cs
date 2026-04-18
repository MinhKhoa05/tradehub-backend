using TradeHub.DAL.Entities;

namespace TradeHub.DAL.Repositories.Interfaces
{
    public interface IOrderItemRepository
    {
        Task<List<OrderItem>> GetByOrderIdAsync(int orderId);
        Task<int> CreateRangeAsync(IEnumerable<OrderItem> items);
    }
}
