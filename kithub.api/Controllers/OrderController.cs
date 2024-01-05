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
        #region Constructor
        private readonly IOrderRepository _orderRepository;

        public OrderController(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }
        #endregion

        #region Get order details
        [HttpGet]
        [Route("{orderId:int}")]
        public async Task<ActionResult<OrderDto>> GetOrderDetails(int orderId)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var order = await _orderRepository.GetOrderDetail(orderId.ToString());
                if (order is null || userId != order.UserId)
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
        #endregion

        #region Fetch all orders for the logged in user
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
        #endregion

        #region Create New Order
        [HttpPost]
        [Route("CreateOrder")]
        public async Task<ActionResult<int>> CreateOrder(OrderDto orderDto)
        {
            try
            {
                string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                var order = await _orderRepository.CreateOrder(userId, orderDto);

                return Ok(order.Id);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }
        #endregion
    }
}
