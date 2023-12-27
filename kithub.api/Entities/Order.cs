namespace kithub.api.Entities
{
    public class Order
    {
        public Guid Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public int Amount { get; set; }
        public string Status { get; set; } = "ORDER_CREATED";
        public List<OrderItem> OrderItems { get; set; } = new();
        public string Checksum {  get; set; } =string.Empty;
        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedOn { get; set; }
    }
}
