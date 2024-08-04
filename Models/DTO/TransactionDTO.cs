namespace SMS.Models.DTO
{
    public class TransactionDTO
    {
        public int TransactionId { get; set; }
        public DateTime? CreatedAt { get; set; }
        public decimal SubTotal { get; set; }
        public decimal Interest { get; set; }
        public decimal TotalAmount { get; set; }
        public GetCustomerDTO Customer { get; set; }
        public ICollection<TransactionItemDTO> TransactionItems { get; set; }
    }

    public class CreateTransactionDTO
    {
        public string CustomerNIC { get; set; }
        public DateTime Date { get; set; }
        public bool PaymentStatus { get; set; }
        public decimal SubTotal { get; set; }
        public decimal Interest { get; set; }
        public decimal TotalAmount { get; set; }
        public List<CreateItemDTO> Items { get; set; }
    }

    public class TransactionItemDTO
    {
        public int TransactionItemId { get; set; }
        public int TransactionId { get; set; }
        public int ItemId { get; set; }
        public GetItemDTO Item { get; set; }
    }

    public class UpdateTransactionDTO
    {
        public string Type { get; set; }
        public string Description { get; set; }
        public decimal Amount { get; set; }
        public DateTime Date { get; set; }
    }
}
