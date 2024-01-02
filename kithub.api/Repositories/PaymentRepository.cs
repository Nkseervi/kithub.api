using kithub.api.Data;
using kithub.api.models.PhonePe;
using Newtonsoft.Json;
using System.Buffers.Text;
using System.Text;
using System.Text.Json;

namespace kithub.api.Repositories
{
    public class PaymentRepository : IPaymentRepository
    {
        #region Constructor
        private readonly KithubDbContext _dbContext;
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger _logger;

        private System.Timers.Timer timer = new System.Timers.Timer(20000); //1st @20-25 sec
        public PaymentRepository(KithubDbContext dbContext,
                                    HttpClient httpClient,
                                    IConfiguration configuration,
                                    ILogger<PaymentRepository> logger)
        {
            _dbContext = dbContext;
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;
        }
        #endregion

        #region Generate Payment Link for PhonePe
        public async Task<PayApiResponse> GeneratePaymentLink(string xverify, string base64)
        {
            try
            {
                // ON LIVE URL YOU MAY GET CORS ISSUE, ADD Below LINE TO RESOLVE
                //ServicePointManager.Expect100Continue = true;
                //ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                string api = _configuration.GetValue<string>("PhonePe:PayApiGatewayUrl") +
                                                     _configuration.GetValue<string>("PhonePe:PayApiEndPoint");

                var uri = new Uri(api);

                _httpClient.DefaultRequestHeaders.Add("accept", "application/json");
                _httpClient.DefaultRequestHeaders.Add("X-VERIFY", xverify);

                var jsonBody = $"{{\"request\":\"{base64}\"}}";
                var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(uri, content);

                if (response.IsSuccessStatusCode == false)
                {
                    return null;
                }

                // Read and deserialize the response content
                var responseContent = await response.Content.ReadAsStringAsync();
                return System.Text.Json.JsonSerializer.Deserialize<PayApiResponse>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            }
            catch (Exception)
            {
                //Log exception
                return null;
            }
        }

        public string generatePhonePePayload(OrderDto order)
        {
            Payload payload = new Payload()
            {
                amount = (int)decimal.Multiply(order.Amount,100),
                callbackUrl = _configuration.GetValue<string>("PhonePe:CallbackUrl"),
                merchantId = _configuration.GetValue<string>("PhonePe:MerchantId"),
                merchantTransactionId = order.Id.ToString(),
                merchantUserId = "MUID123",
                mobileNumber = _configuration.GetValue<string>("PhonePe:MobileNumber"),
                redirectMode = "REDIRECT",
                redirectUrl = string.Concat(_configuration.GetValue<string>("PhonePe:RedirectUrl"),
                                        order.Id),
                paymentInstrument = new PayloadPaymentInstrument
                {
                    type = "PAY_PAGE"
                }
            };

            return ProcessPaymentData.ConvertToBase64(payload);
        }

        #endregion

        public async Task StartTimer(Order order)
        {
            timer.AutoReset = true;
            timer.Elapsed += async (_s, _e) => await CheckStatus(order);
            timer.Enabled = true;
            timer.Start();
        }

        private async Task CheckStatus(Order order)
        {
            _logger.LogInformation($"Timer interval {timer.Interval} at \t{DateTime.Now}");

            string orderStatus = order.Status;

            if (order.Status == "PAYMENT_PENDING" ||
                                        order.Status == "ORDER_CREATED" ||
                                        order.Status == "PAYMENT_INITIATED")
            {
                try
                {
                    // ON LIVE URL YOU MAY GET CORS ISSUE, ADD Below LINE TO RESOLVE
                    //ServicePointManager.Expect100Continue = true;
                    //ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                    string api = _configuration.GetValue<string>("PhonePe:PayApiGatewayUrl") +
                                                _configuration.GetValue<string>("PhonePe:CheckStatusApiEndPoint") +
                                                _configuration.GetValue<string>("PhonePe:MerchantId") + "/" +
                                                order.Id.ToString();
                    var uri = new Uri(api);
                    string reqString = string.Concat(_configuration.GetValue<string>("PhonePe:CheckStatusApiEndPoint"),
                                            _configuration.GetValue<string>("PhonePe:MerchantId"), "/", order.Id.ToString(),
                                            _configuration.GetValue<string>("PhonePe:SaltKey"));
                    string xverify = reqString.GenerateHash();

                    HttpClient httpClient = new HttpClient();
                    // Add headers
                    httpClient.DefaultRequestHeaders.Add("accept", "application/json");
                    httpClient.DefaultRequestHeaders.Add("X-VERIFY", xverify);
                    httpClient.DefaultRequestHeaders.Add("X-MERCHANT-ID", _configuration.GetValue<string>("PhonePe:MerchantId"));

                    // Send POST request
                    var response = await httpClient.GetAsync(uri);
                    response.EnsureSuccessStatusCode();

                    // Read and deserialize the response content
                    var responseContent = await response.Content.ReadAsStringAsync();

                    var resp = System.Text.Json.JsonSerializer.Deserialize<CheckStatusResponse>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    var optionsBuilder = new DbContextOptionsBuilder<KithubDbContext>();
                    optionsBuilder.UseSqlServer(_configuration.GetValue<string>("ConnectionStrings:KithubAPIContext"));
                    KithubDbContext kithubDbContext = new KithubDbContext(optionsBuilder.Options);

                    if (resp.code != "PAYMENT_PENDING")
                    {
                        var item = await kithubDbContext.Orders
                                .Include(p => p.Payment)
                                .SingleOrDefaultAsync(p => p.Id == order.Id);
                        item.Status = resp.code;
                        item.UpdatedOn = DateTime.UtcNow;
                        item.Payment.Status = resp.code;
                        item.Payment.CheckstatusRequest = reqString;
                        item.Payment.CheckstatusResponse = responseContent;
                        item.Payment.LastModified = DateTime.UtcNow;
                        await kithubDbContext.SaveChangesAsync();
                    }
                    httpClient.Dispose();
                    kithubDbContext.Dispose();
                    //return api;
                    // Return a response
                    //return Json(new { Success = true, Message = "Verification successful", phonepeResponse = responseContent });
                }
                catch (Exception ex)
                {
                    // Handle errors and return an error response
                    //return Json(new { Success = false, Message = "Verification failed", Error = ex.Message });
                }
            }

            if (orderStatus == "PAYMENT_PENDING" || orderStatus == "PAYMENT_INITIATED")
            {
                var timeElapsed = DateTime.UtcNow.Subtract(order.CreatedOn);
                _logger.LogInformation($"Time elapsed \t {timeElapsed.TotalSeconds} Sec \t{timeElapsed.TotalMinutes} Min");

                if (timeElapsed.TotalSeconds < 20)
                {

                }
                else if (timeElapsed.TotalSeconds < 50) // every 3 sec
                {
                    timer.Interval = 3000;
                }
                else if (timeElapsed.TotalSeconds < 110) // every 6 sec
                {
                    timer.Interval = 6000;
                }
                else if (timeElapsed.TotalSeconds < 170) // every 10 sec
                {
                    timer.Interval = 10000;
                }
                else if (timeElapsed.TotalSeconds < 230) // every 30 sec
                {
                    timer.Interval = 30000;
                }
                else if (timeElapsed.TotalMinutes < 20) // every minute till 20mins
                {
                    timer.Interval = 60000;
                }
                else
                {
                    timer.Stop();
                    timer.Dispose();
                    _logger.LogInformation($"Stopped timer at {DateTime.Now}");
                }
            }
            else
            {
                timer.Stop();
                timer.Dispose();
                _logger.LogInformation($"Stopped timer at {DateTime.Now}");
            }
        }

    }
}
