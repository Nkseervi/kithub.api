using kithub.api.Entities;
using kithub.api.models.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NuGet.Protocol;
using System.Security.Claims;

namespace kithub.api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class OrderController : ControllerBase
    {
        private readonly IOrderRepository _orderRepository;

        public OrderController(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        [HttpGet]
        [Route("{orderId:int}")]
        public async Task<ActionResult<OrderDto>> GetOrderDetails(int orderId)
        {
            try
            {
                var order = await _orderRepository.GetOrderDetail(orderId.ToString());
                if (order is null)
                {
                    return NoContent();
                }

                var orderDto = order.ConvertToDto();

                return Ok(orderDto);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<OrderDto>>> GetAllOrders()
        {
            try
            {
                string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var orders = await _orderRepository.GetAllOrders(userId);
                if (orders is null)
                {
                    return NoContent();
                }

                var ordersDto = orders.ConvertToDto();

                return Ok(ordersDto);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpPost]
        [Route("PhonePeCallback")]
        [AllowAnonymous]
        public async Task<ActionResult<string>> PhonePeCallback([FromHeader(Name = "X-Verify")] string xverify,
                                                                [FromBody] PhonePayCallback callbackResponse)
        {
            try
            {
                if (_orderRepository.VerifyCheckSum(xverify, callbackResponse.response))
                {
                    var decodedCallbackResponse = _orderRepository.DecodeBase64Response(callbackResponse.response);

                    if (decodedCallbackResponse is null)
                    {
                        return NoContent();
                    }

                    var order = await _orderRepository
                                    .GetOrderDetail(decodedCallbackResponse.data.merchantTransactionId);
                    
                    var updatedStatus = await _orderRepository.UpdateOrderStatus(order, decodedCallbackResponse.code, string.Empty);

                    return Ok(updatedStatus);
                }
                else 
                {
                    return BadRequest();
                }
            }
            catch(Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpPost]
        [Route("GeneratePaymentLink")]
        public async Task<ActionResult<Uri>> GeneratePaymentLink(OrderDto orderDto)
        {
            try
            {
                string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                var url = await _orderRepository.GeneratePaymentLink(userId, orderDto.Amount);
                return Ok(url);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpPost]
        [Route("CheckPaymentStatus/{orderId:int}")]
        public async Task<IActionResult> CheckPaymentStatus(int orderId)
        {
            try
            {
                var url = await _orderRepository.CheckPaymentStatus(orderId);
                return Ok(url);
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

    }
}
