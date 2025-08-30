using System.ComponentModel.DataAnnotations;

namespace SMS.Models.DTO
{
    public class GetInvoiceDTO
    {
        public int InvoiceId { get; set; }
        public int TransactionId { get; set; }
        public string InvoiceNo { get; set; } = string.Empty;
        public DateTime? DateGenerated { get; set; }
        public int? Status { get; set; }
        public int InvoiceTypeId { get; set; }
        public string? CustomerName { get; set; }
        public string? CustomerNIC { get; set; }
        public decimal TotalAmount { get; set; }
        
        // Legacy properties for backward compatibility
        public decimal? PrincipleAmount { get; set; }
        public decimal? InterestRate { get; set; }
        public decimal? InterestAmount { get; set; }
        public int? LoanPeriod { get; set; }
    }

    public class CreateInvoiceDTO
    {
        [Required]
        public int TransactionId { get; set; }
        [Required]
        public int InvoiceTypeId { get; set; }
        public int? Status { get; set; }
        
        // Legacy properties for business logic compatibility
        public CreateCustomerDTO? Customer { get; set; }
        public DateTime Date { get; set; } = DateTime.Now;
        public CustomItemDTO[]? Items { get; set; }
        public decimal? SubTotal { get; set; }
        public decimal? InterestRate { get; set; }
        public decimal? InterestAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public int? LoanPeriodId { get; set; }
    }

    public class UpdateInvoiceDTO
    {
        [Required]
        public int TransactionId { get; set; }
        [Required]
        public int InvoiceTypeId { get; set; }
        public int? Status { get; set; }
    }

    public class InvoiceSearchRequest : DateRangeRequest
    {
        public string? CustomerNIC { get; set; }
        public int? Status { get; set; }
        public int? InvoiceTypeId { get; set; }
    }

    public class LoanInfoDTO
    {
        public decimal? PrincipleAmount { get; set; }
        public decimal? InterestRate { get; set; }
        public decimal? InterestAmount { get; set; }
        public decimal? DailyInterest { get; set; }
        public decimal TotalAmount { get; set; }
        public int LoanPeriod { get; set; }
        public bool IsLoanSettled { get; set; }
        public DateTime? LastInstallmentDate { get; set; }
    }

    // Legacy DTO for backward compatibility
    public class LoanInfo
    {
        public string InvoiceNumber { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal LoanAmount { get; set; }
        public decimal? PrincipleAmount { get; set; }
        public decimal? InterestRate { get; set; }
        public decimal? InterestAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal? DailyInterest { get; set; }
        public int LoanPeriod { get; set; }
        public decimal? PaidAmount { get; set; }
        public decimal? BalanceAmount { get; set; }
        public bool IsLoanSettled { get; set; }
        public DateTime? LastInstallmentDate { get; set; }
        public List<LegacyInstallmentDTO> Installments { get; set; } = new List<LegacyInstallmentDTO>();
    }

    // Legacy DTO for backward compatibility  
    public class LegacyInstallmentDTO
    {
        public int InstallmentId { get; set; }
        public decimal InstallmentAmount { get; set; }
        public DateTime? PaymentDate { get; set; }
        public bool IsPaid { get; set; }
    }
}