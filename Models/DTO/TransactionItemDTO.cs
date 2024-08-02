namespace SMS.Models.DTO
{
    public class GetTransactionItemDTO
    {
        public int TransactionItemId { get; set; }
        public int TransactionId { get; set; }
        public int ItemId { get; set; }

    }
    public class CreateTransactionItemDTO
    {
        public int TransactionId { get; set; }
        public int ItemId { get; set; }

    }
    public class UpdateTransactionItemDTO
    { 
        public int TransactionId { get; set; }
        public int ItemId { get; set; }

    }
}
