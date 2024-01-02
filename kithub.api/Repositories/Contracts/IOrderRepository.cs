using kithub.api.models.PhonePe;

namespace kithub.api.Repositories.Contracts
{
    public interface IOrderRepository
    {
        Task<Order> CreateOrder(string userId, OrderDto orderDto);
        Task<IEnumerable<Order>> GetAllOrders(string userId);
        Task<Order> GetOrderDetail(string orderId);
        Task UpdateOrderStatus(Order order);
    }
}