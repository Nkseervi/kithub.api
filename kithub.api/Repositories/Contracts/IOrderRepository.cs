using kithub.api.models.PhonePe;

namespace kithub.api.Repositories.Contracts
{
    public interface IOrderRepository
    {
        Task<string> CheckPaymentStatus(Order order);
        Task<Order> CreateOrder(string userId, OrderDto orderDto);
        CallbackResponse DecodeBase64Response(string callbackResponse);
        Task<Uri> GeneratePaymentLink(OrderDto orderDto);
        Task<IEnumerable<Order>> GetAllOrders(string userId);
        Task<Order> GetOrderDetail(Guid orderId);
        Task<string> UpdateOrderStatus(Order order, string status);
    }
}