namespace SMS.Models.DTO
{
    public class ReportDTO
    {
        public int CustomerId { get; set; }
        public string CustomerName { get; set; }
        public string CustomerNIC { get; set; }

        public List<LoanDTO> Loans { get; set; } // Includes loans and their installments
    }

    public class LoanDTOs
    {
        public int LoanId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal AmountPaid { get; set; }
        public decimal OutstandingAmount { get; set; }

        public Boolean IsSettled { get; set; }

        public List<InstallmentDTO> Installments { get; set; } // List of installments associated with this loan
    }

    public class InstallmentDTOs
    {
        public int InstallmentId { get; set; }
        public int InstallmentNumber { get; set; }
        public decimal Amount { get; set; }
        public bool IsPaid { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime? PaymentDate { get; set; } // Nullable to represent unpaid installments
    }

    public class TransactionReportDTO
    {
        public int TransactionId { get; set; }
        public TransactionType TransactionType { get; set; }
        public DateTime? CreatedAt { get; set; }
        public decimal? SubTotal { get; set; }
        public decimal? InterestRate { get; set; }
        public decimal? InterestAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public InvoiceReportDTO Invoice { get; set; }
        public List<InstallmentReportDTO> Installments { get; set; }
        public CustomerReportDTO Customer { get; set; }
    }

    public class InvoiceReportDTO
    {
        public int InvoiceId { get; set; }
        public InvoiceType InvoiceTypeId { get; set; }
        public string InvoiceNo { get; set; }
        public DateTime? DateGenerated { get; set; }
        public int? Status { get; set; }
    }

    public class InstallmentReportDTO
    {
        public int InstallmentId { get; set; }
        public int InstallmentNumber { get; set; }
        public decimal AmountPaid { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime? PaymentDate { get; set; }
    }

    public class CustomerReportDTO
    {

        public string CustomerNIC { get; set; }
        public string? CustomerName { get; set; }
        public string? CustomerAddress { get; set; }
        public string? CustomerContactNo { get; set; }
    }


    public class Overview
    {
        public int? TotalActiveLoans { get; set; }

        public int? TotalInvoices { get; set; }

        public decimal? RevenueGenerated { get; set; }

        public int? InventoryCount { get; set; }

        public int? CustomerCount { get; set; }

    }
}
