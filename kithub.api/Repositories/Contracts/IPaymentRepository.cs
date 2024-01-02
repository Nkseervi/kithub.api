using kithub.api.models.PhonePe;

namespace kithub.api.Repositories.Contracts
{
    public interface IPaymentRepository
    {
        Task<PayApiResponse> GeneratePaymentLink(string xverify, string base64);
        string generatePhonePePayload(OrderDto order);
        Task StartTimer(Order order);
    }
}