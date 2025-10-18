using System.ComponentModel.DataAnnotations;

namespace SMS.Models.DTO
{
    public class TransactionItemDTO
    {
        public int TransactionItemId { get; set; }
        public int TransactionId { get; set; }
        public int ItemId { get; set; }
        public decimal? PawnValue { get; set; }
        public string? ItemDescription { get; set; }
    }

    public class GetTransactionItemDTO
    {
        public int TransactionItemId { get; set; }
        public int TransactionId { get; set; }
        public int ItemId { get; set; }
        public decimal? PawnValue { get; set; }
    }

    public class UpdateTransactionItemDTO
    {
        [Required]
        public int ItemId { get; set; }
        public decimal? PawnValue { get; set; }
    }
}
