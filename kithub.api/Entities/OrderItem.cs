namespace kithub.api.Entities
{
    public class OrderItem
    {
        public int Id { get; set; }
        public string ProductName { get; set; }
        public string ProductDescription { get; set; }
        public int GstRate { get; set; }
        public decimal ListedPrice { get; set; }
        public int Discount { get; set; }
        public decimal SellingPrice { get; set; }
        public decimal TotalPrice { get; set; }
        public int Qty { get; set; }
    }
}
