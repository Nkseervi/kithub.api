using kithub.api.Entities;
using kithub.api.models.Dtos;
using kithub.api.models.PhonePe;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using NuGet.Packaging.Signing;
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
        #region Constructor
        private readonly KithubDbContext _kithubDbContext;
        private readonly IConfiguration _config;
        private readonly HttpClient _client;
        private readonly ILogger _logger;

        public OrderRepository(KithubDbContext kithubDbContext,
                                IConfiguration config,
                                HttpClient client,
                                ILogger<OrderRepository> logger)
        {
            _kithubDbContext = kithubDbContext;
            _config = config;
            _client = client;
            _logger = logger;
        }
        #endregion

        #region Get order details
        public async Task<Order> GetOrderDetail(string orderId)
        {
            var order = await _kithubDbContext.Orders
                                        .Include(o => o.OrderItems)
                                        .FirstOrDefaultAsync(o => o.Id.ToString() == orderId);
            return order;
        }
        #endregion

        #region Fetch all Orders for the user
        public async Task<IEnumerable<Order>> GetAllOrders(string userId)
        {
            var orders = await _kithubDbContext.Orders
                                                .Include(o => o.OrderItems)
                                                .Where(o => o.UserId == userId)
                                                .OrderByDescending(o => o.CreatedOn)
                                                .ToListAsync();
            return orders;
        }
        #endregion

        #region Create new order
        public async Task<Order> CreateOrder(string userId, OrderDto orderDto)
        {
            Order order = new()
            {
                Amount = orderDto.Amount,
                UserId = userId,
                OrderItems = (from item in orderDto.OrderItems
                              select new OrderItem
                              {
                                  ProductName = item.ProductName,
                                  ProductDescription = item.ProductDescription,
                                  GstRate = item.GstRate,
                                  ListedPrice = item.ListedPrice,
                                  Qty = item.Qty,
                                  Discount = item.DiscountPercent,
                                  SellingPrice = item.SellingPrice,
                              }).ToList(),
                Payment = new Payment
                {
                    Status = orderDto.Payment.Status,
                    Created = DateTime.UtcNow,
                    Amount = orderDto.Payment.Amount,
                    Gateway = orderDto.Payment.Gateway
                }
            };
            var result = await _kithubDbContext.Orders.AddAsync(order);
            await _kithubDbContext.SaveChangesAsync();
            return result.Entity;
        }
        #endregion

        #region Update Order details
        public async Task UpdateOrderStatus(Order order)
        {
            try
            {
                var item = await _kithubDbContext.Orders
                                .Include(p => p.Payment)
                                .SingleOrDefaultAsync(p => p.Id == order.Id);
                item.Status = order.Status;
                item.UpdatedOn = DateTime.UtcNow;
                item.Payment.Status = order.Payment.Status;
                item.Payment.Request = order.Payment.Request;
                item.Payment.Response = order.Payment.Response;
                item.Payment.CheckstatusRequest = order.Payment.CheckstatusRequest;
                item.Payment.CheckstatusResponse = order.Payment.CheckstatusResponse;
                item.Payment.Callback = order.Payment.Callback;
                await _kithubDbContext.SaveChangesAsync();                
            }
            catch (Exception)
            {
                throw;
            }
        }
        #endregion
        
    }
}
