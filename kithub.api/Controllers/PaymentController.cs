using Azure;
using kithub.api.models.PhonePe;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Buffers.Text;
using System.Security.Claims;
using System.Text;
using static kithub.api.Controllers.OrderController;

namespace kithub.api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PaymentController : ControllerBase
    {
        #region Constructor
        private readonly IPaymentRepository _paymentRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly IConfiguration _configuration;

        public PaymentController(IPaymentRepository paymentRepository,
                                    IOrderRepository orderRepository,
                                    IConfiguration configuration)
        {
            _paymentRepository = paymentRepository;
            _orderRepository = orderRepository;
            _configuration = configuration;
        }
        #endregion

        #region Generate Payment Link
        [HttpPost]
        public async Task<ActionResult<Uri>> GeneratePaymentLink(OrderDto orderDto)
        {
            try
            {
                string base64 = _paymentRepository.generatePhonePePayload(orderDto);

                string xverify = (string.Concat(base64,
                                        _configuration.GetValue<string>("PhonePe:PayApiEndPoint"),
                                        _configuration.GetValue<string>("PhonePe:SaltKey")))
                                        .GenerateHash();


                var response = await _paymentRepository.GeneratePaymentLink(xverify,base64);

                Order order = new Order
                {
                    Id = orderDto.Id,
                    Status = response.code,
                    UpdatedOn = DateTime.UtcNow,
                    Payment = new Payment
                    {
                        Request = base64,
                        Response = JsonConvert.SerializeObject(response),
                        Status = response.code,
                        LastModified = DateTime.UtcNow
                    }
                };
                await _orderRepository.UpdateOrderStatus(order);

                await _paymentRepository.StartTimer(order);

                return Ok(response.data.instrumentResponse.redirectInfo.url);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }
        #endregion

        #region Process PhonePe Callbacks
        [HttpPost]
        [Route("PhonePeCallback")]
        [AllowAnonymous]
        public async Task<ActionResult<string>> PhonePeCallback([FromHeader(Name = "X-Verify")] string xverify,
                                                                [FromBody] PhonePayCallback callbackResponse)
        {
            try
            {
                if (xverify == (callbackResponse + _configuration.GetValue<string>("PhonePe:SaltKey")).GenerateHash())
                {
                    var base64EncodedBytes = Convert.FromBase64String(callbackResponse.response);
                    var inputString = Encoding.UTF8.GetString(base64EncodedBytes);
                    CallbackResponse decodedCallback = JsonConvert.DeserializeObject<CallbackResponse>(inputString);

                    Payment payment = new Payment
                    {
                        Callback = inputString,
                        Status = decodedCallback.code,
                        LastModified = DateTime.UtcNow
                    };

                    Order order = new Order
                    {
                        Id = Convert.ToInt32(decodedCallback.data.merchantTransactionId),
                        UpdatedOn = DateTime.UtcNow,
                        Status = decodedCallback.code,
                        Payment = new Payment
                        {
                            Callback = inputString,
                            Status = decodedCallback.code,
                            LastModified = DateTime.UtcNow
                        }
                    };
                    await _orderRepository.UpdateOrderStatus(order);
                }

                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        public record PhonePayCallback
        {
            public string response { get; set; }
        } 
        #endregion

    }
}
