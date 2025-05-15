namespace JwtTechTask.Models
{
    public class Transaction
    {
        public string Id { get; set; }
        public decimal? Amount { get; set; }
        public string Currency { get; set; }
        public Meta Meta { get; set; }
        public TransactionStatus Status { get; set; }
    }
}