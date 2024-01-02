namespace kithub.api.Entities
{
    public class Payment
    {
        public int Id { get; set; }
        public string Status { get; set; }
        public DateTime Created { get; set; } = DateTime.UtcNow;
        public decimal Amount { get; set; }
        public string? Gateway { get; set; }
        public string? Request { get; set; }
        public string? Response { get; set; }
        public string? Callback { get; set; }
        public string? CheckstatusRequest { get; set; }
        public string? CheckstatusResponse { get; set; }
        public DateTime? LastModified { get; set; }
    }
}
