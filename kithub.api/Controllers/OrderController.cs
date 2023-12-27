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

        [HttpPost]
        public async Task<ActionResult<string>> CreateOrder(OrderDto orderDto)
        {
            try
            {
                string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var newOrder = await _orderRepository.CreateOrder(userId, orderDto);
                if (newOrder is null)
                {
                    return NoContent();
                }

                return Ok(newOrder.Id.ToString());
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpGet]
        [Route("/GetItems")]
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
        [Route("/PhonePeCallback")]
        [AllowAnonymous]
        public async Task<ActionResult<string>> PhonePeCallback([FromHeader(Name = "X-Verify")] string xverify,
                                                                [FromForm(Name = "response")] string callbackResponse)
        {
            try
            {
                var decodedCallbackResponse = _orderRepository.DecodeBase64Response(callbackResponse);

                if (decodedCallbackResponse is null)
                {
                    return NoContent();
                }

                var order = await _orderRepository
                                .GetOrderDetail(new Guid(decodedCallbackResponse.data.merchantTransactionId));
                if (order is null || order.Checksum != xverify)
                {
                    return BadRequest();
                }

                var updatedStatus = await _orderRepository.UpdateOrderStatus(order, decodedCallbackResponse.code);

                return Ok(updatedStatus);
            }
            catch(Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpGet]
        [Route("/GeneratePaymentLink")]
        public async Task<ActionResult<Uri>> GeneratePaymentLink(OrderDto orderDto)
        {
            try
            {
                var url = await _orderRepository.GeneratePaymentLink(orderDto);
                return Ok(url);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

    }
}
