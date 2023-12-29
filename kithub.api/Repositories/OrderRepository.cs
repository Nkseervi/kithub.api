using kithub.api.models.Dtos;
using kithub.api.models.PhonePe;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Buffers.Text;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using static System.Net.WebRequestMethods;

namespace kithub.api.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly KithubDbContext _kithubDbContext;
        private readonly IConfiguration _config;
        private readonly HttpClient _client;

        public OrderRepository(KithubDbContext kithubDbContext,
                                IConfiguration config,
                                HttpClient client)
        {
            _kithubDbContext = kithubDbContext;
            _config = config;
            _client = client;
        }

        public async Task<Order> GetOrderDetail(string orderId)
        {
            var order = await _kithubDbContext.Orders
                                        .FirstOrDefaultAsync(o => o.Id.ToString() == orderId);
            return order;
        }

        public async Task<IEnumerable<Order>> GetAllOrders(string userId)
        {
            var orders = await _kithubDbContext.Orders
                                                .Include(o => o.OrderItems)
                                                .Where(o => o.UserId == userId)
                                                .OrderByDescending(o => o.CreatedOn)
                                                .ToListAsync();
            return orders;
        }

        private async Task<Order> CreateOrder(string userId, int payAmount)
        {
            Order order = new()
            {
                AmountPaise = payAmount,
                UserId = userId
            };
            var result = await _kithubDbContext.Orders.AddAsync(order);
            await _kithubDbContext.SaveChangesAsync();
            return result.Entity;
        }

        public async Task<string> CheckPaymentStatus(int orderId)
        {
            var item = await _kithubDbContext.Orders.FirstOrDefaultAsync(o => o.Id == orderId);
            if (item is not null)
            {
                try
                {
                    // ON LIVE URL YOU MAY GET CORS ISSUE, ADD Below LINE TO RESOLVE
                    //ServicePointManager.Expect100Continue = true;
                    //ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                    string api = _config.GetValue<string>("PhonePe:PayApiGatewayUrl") + "/" +
                                                _config.GetValue<string>("PhonePe:CheckStatusApiEndPoint") +
                                                _config.GetValue<string>("PhonePe:MerchantId") + "/" +
                                                orderId.ToString();
                    var uri = new Uri(api);
                    string reqString = string.Concat(_config.GetValue<string>("PhonePe:CheckStatusApiEndPoint"),
                                            _config.GetValue<string>("MerchantId"), "/", orderId.ToString(),
                                            _config.GetValue<string>("PhonePe:SaltKey"));
                    string xverify = genarateXVerify(reqString);
                    // Add headers
                    _client.DefaultRequestHeaders.Add("accept", "application/json");
                    _client.DefaultRequestHeaders.Add("X-VERIFY", xverify);
                    _client.DefaultRequestHeaders.Add("X-MERCHANT-ID", _config.GetValue<string>("MerchantId"));

                    // Send POST request
                    var response = await _client.GetAsync(uri);
                    response.EnsureSuccessStatusCode();

                    // Read and deserialize the response content
                    var responseContent = await response.Content.ReadAsStringAsync();

                    //return api;
                    // Return a response
                    //return Json(new { Success = true, Message = "Verification successful", phonepeResponse = responseContent });
                }
                catch (Exception ex)
                {
                    // Handle errors and return an error response
                    //return Json(new { Success = false, Message = "Verification failed", Error = ex.Message });
                }

                item.UpdatedOn = DateTime.UtcNow;
                await _kithubDbContext.SaveChangesAsync();
            }
            return item.Status;
        }

        public async Task<Uri> GeneratePaymentLink(string userId, int amount)
        {
            try
            {
                // ON LIVE URL YOU MAY GET CORS ISSUE, ADD Below LINE TO RESOLVE
                //ServicePointManager.Expect100Continue = true;
                //ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                var order = await CreateOrder(userId, amount);

                string base64 = generateBase64Payload(order);
                string reqString = string.Concat(base64,
                                                    _config.GetValue<string>("PhonePe:PayApiEndPoint"),
                                                    _config.GetValue<string>("PhonePe:SaltKey"));
                string xverify = genarateXVerify(reqString);

                var response = await SendRequest(base64, xverify);

                if (response is not null)
                {
                    if (response.success)
                    {
                        await UpdateOrderStatus(order, "PAYMENT_INITIATED", xverify);
                        return new Uri(response.data.instrumentResponse.redirectInfo.url);

                    }
                }
                return null;
            }
            catch (Exception)
            {
                //Log exception
                return null;
            }
        }

        public async Task<string> UpdateOrderStatus(Order order, string status, string checksum)
        {
            try
            {
                order.Status = status;
                order.UpdatedOn = DateTime.UtcNow;
                if(string.IsNullOrWhiteSpace(order.Checksum))
                {
                    order.Checksum = checksum;
                }                
                await _kithubDbContext.SaveChangesAsync();
                return status;
            }
            catch (Exception) 
            {
                throw;
            }
        }
        public CallbackResponse DecodeBase64Response(string callbackResponse)
        {
            try
            {
                var base64EncodedBytes = Convert.FromBase64String(callbackResponse);
                var inputString = Encoding.UTF8.GetString(base64EncodedBytes);
                var callbackObject = JsonConvert.DeserializeObject<CallbackResponse>(inputString);
                return callbackObject;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        private string generateBase64Payload(Order order)
        {
            Payload payload = new Payload()
            {
                amount = order.AmountPaise,
                callbackUrl = _config.GetValue<string>("PhonePe:CallbackUrl"),
                //callbackUrl = "https://webhook.site/732c1622-8f76-40ae-b1c1-988d2e15600b",
                merchantId = _config.GetValue<string>("PhonePe:MerchantId"),
                merchantTransactionId = order.Id.ToString(),
                //merchantTransactionId = "MT7850590068188104",
                //merchantUserId = order.UserId,
                merchantUserId = "MUID123",
                mobileNumber = "9999999999",
                redirectMode = "REDIRECT",
                redirectUrl = string.Concat(_config.GetValue<string>("PhonePe:RedirectUrl"),
                                        order.Id),
                paymentInstrument = new PayloadPaymentInstrument
                {
                    type = "PAY_PAGE"
                }
            };
            var wncoded = JsonConvert.SerializeObject(payload);
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(wncoded);
            var base64 = System.Convert.ToBase64String(plainTextBytes);

            return base64;
        }

        private string genarateXVerify(string reqString)
        {                
            SHA256 sha256Hash = SHA256.Create();
            var xfiles = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(reqString));
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < xfiles.Length; i++)
            {
                builder.Append(xfiles[i].ToString("x2"));
            }
            builder.Append("###1");
            string xverify = builder.ToString();

            return xverify;
        }

        private async Task<PayApiResponse> SendRequest(string base64, string xverify)
        {
            string api = _config.GetValue<string>("PhonePe:PayApiGatewayUrl") +
                                                _config.GetValue<string>("PhonePe:PayApiEndPoint");

            var uri = new Uri(api);

            // Add headers
            _client.DefaultRequestHeaders.Add("accept", "application/json");
            _client.DefaultRequestHeaders.Add("X-VERIFY", xverify);

            // Create JSON request body
            var jsonBody = $"{{\"request\":\"{base64}\"}}";
            var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

            // Send POST request
            var response = await _client.PostAsync(uri, content);

            

            if (response.IsSuccessStatusCode == false)
            {
                return null;
            }

            // Read and deserialize the response content
            var responseContent = await response.Content.ReadAsStringAsync();
            return System.Text.Json.JsonSerializer.Deserialize<PayApiResponse>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        }

        public bool VerifyCheckSum(string xverify, string payload)
        {
            string result = genarateXVerify(payload + _config.GetValue<string>("PhonePe:SaltKey"));
            if(result == xverify)
                return true;
            else return false;
        }
    }
}
