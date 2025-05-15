namespace JwtTechTask.Models
{
    public class User
    {
        public string UserId { get; set; }
        public List<Transaction> Transactions { get; set; }
    }
}
