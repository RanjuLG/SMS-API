namespace SMS.Models.DTO
{
    public class TransactionDTO
    {
        public int TransactionId { get; set; }
        public DateTime? CreatedAt { get; set; }
        public decimal? SubTotal { get; set; }
        public decimal? InterestRate { get; set; }
        public decimal? InterestAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public GetCustomerDTO Customer { get; set; }
        public ICollection<GetItemDTO> Items { get; set; } // Add this line to include the items
    }

    public class CreateTransactionDTO
    {
        public string CustomerNIC { get; set; }
        public DateTime Date { get; set; }
        public bool PaymentStatus { get; set; }
        public decimal? SubTotal { get; set; }
        public decimal? InterestRate { get; set; }
        public decimal? InterestAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public List<CreateItemDTO> Items { get; set; }
    }

    public class UpdateTransactionDTO
    {
        public string Type { get; set; }
        public string Description { get; set; }
        public decimal Amount { get; set; }
        public DateTime Date { get; set; }
    }
}
