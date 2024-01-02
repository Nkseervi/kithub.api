namespace kithub.api.Entities
{
    public class Order
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public decimal Amount { get; set; }
        public string Status { get; set; } = "ORDER_CREATED";
        public List<OrderItem> OrderItems { get; set; } = new();
        public Payment Payment { get; set; }
        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedOn { get; set; }
    }
}
