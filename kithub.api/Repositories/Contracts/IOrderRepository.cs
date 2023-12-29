using kithub.api.models.PhonePe;

namespace kithub.api.Repositories.Contracts
{
    public interface IOrderRepository
    {
        Task<string> CheckPaymentStatus(int orderId);
        CallbackResponse DecodeBase64Response(string callbackResponse);
        Task<Uri> GeneratePaymentLink(string userId, int amount);
        Task<IEnumerable<Order>> GetAllOrders(string userId);
        Task<Order> GetOrderDetail(string orderId);
        Task<string> UpdateOrderStatus(Order order, string status, string checksum);
    }
}