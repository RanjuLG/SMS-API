namespace SMS.Models
{
    public class TransactionItem
    {
        public int TransactionItemId { get; set; }
        public int TransactionId { get; set; }
        public int ItemId { get; set; }
        public DateTime? DeletedAt { get; set; }
        public virtual Transaction Transaction { get; set; }
        public virtual Item Item { get; set; }
    }
}
