namespace kithub.api.Entities
{
    public class OrderItem
    {
        public int Id { get; set; }
        public string ProductName { get; set; }
        public int ListedPrice { get; set; }
        public int Discount { get; set; }
        public int SellingPrice { get; set; }
        public int Qty { get; set; }
    }
}
