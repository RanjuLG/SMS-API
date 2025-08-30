using System.ComponentModel.DataAnnotations;

namespace SMS.Models.DTO
{
    public class GetTransactionDTO
    {
        public int TransactionId { get; set; }
        public int CustomerId { get; set; }
        public string? CustomerName { get; set; }
        public string? CustomerNIC { get; set; }
        public decimal? SubTotal { get; set; }
        public decimal? InterestRate { get; set; }
        public decimal TotalAmount { get; set; }
        public int TransactionType { get; set; }
        public decimal? InterestAmount { get; set; }
        public DateTime? CreatedAt { get; set; }
        public List<TransactionItemDTO>? Items { get; set; }
    }

    public class CreateTransactionDTO
    {
        [Required]
        public int CustomerId { get; set; }
        public decimal? SubTotal { get; set; }
        public decimal? InterestRate { get; set; }
        [Required]
        public decimal TotalAmount { get; set; }
        [Required]
        public int TransactionType { get; set; }
        public decimal? InterestAmount { get; set; }
        public List<CreateTransactionItemDTO>? Items { get; set; }
        
        // Legacy property for backward compatibility
        public string? CustomerNIC { get; set; }
    }

    public class TransactionSearchRequest : DateRangeRequest
    {
        public string? CustomerNIC { get; set; }
        public int? TransactionType { get; set; }
        public decimal? MinAmount { get; set; }
        public decimal? MaxAmount { get; set; }
    }

    public class CreateTransactionItemDTO
    {
        [Required]
        public int ItemId { get; set; }
        public decimal? PawnValue { get; set; }
    }

    // Legacy DTOs for backward compatibility
    public class TransactionDTO
    {
        public int TransactionId { get; set; }
        public int CustomerId { get; set; }
        public decimal? SubTotal { get; set; }
        public decimal? InterestRate { get; set; }
        public decimal TotalAmount { get; set; }
        public int TransactionType { get; set; }
        public decimal? InterestAmount { get; set; }
        public DateTime? CreatedAt { get; set; }
        public GetCustomerDTO? Customer { get; set; }
        public List<GetItemDTO>? Items { get; set; }
    }

    public class UpdateTransactionDTO
    {
        [Required]
        public int CustomerId { get; set; }
        public decimal? SubTotal { get; set; }
        public decimal? InterestRate { get; set; }
        [Required]
        public decimal TotalAmount { get; set; }
        [Required]
        public int TransactionType { get; set; }
        public decimal? InterestAmount { get; set; }
    }
}
