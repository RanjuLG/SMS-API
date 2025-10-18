namespace SMS.Models.DTO
{
    public class CustomerReportDTO
    {
        public int CustomerId { get; set; }
        public string CustomerNIC { get; set; } = string.Empty;
        public string? CustomerName { get; set; }
        public string? CustomerAddress { get; set; }
        public string? CustomerContactNo { get; set; }
        public GetCustomerDTO? Customer { get; set; }
        public List<GetTransactionDTO> Transactions { get; set; } = new List<GetTransactionDTO>();
        public List<GetInvoiceDTO> Invoices { get; set; } = new List<GetInvoiceDTO>();
        public List<GetItemDTO> Items { get; set; } = new List<GetItemDTO>();
        public decimal TotalTransactionAmount { get; set; }
        public int TotalTransactions { get; set; }
        public int TotalItems { get; set; }
        public int TotalInvoices { get; set; }
    }

    public class OverviewReportDTO
    {
        public int TotalCustomers { get; set; }
        public int TotalItems { get; set; }
        public int TotalTransactions { get; set; }
        public int TotalInvoices { get; set; }
        public decimal TotalTransactionAmount { get; set; }
        public decimal TotalOutstandingAmount { get; set; }
        public int ActiveLoans { get; set; }
        public int SettledLoans { get; set; }
        
        public List<MonthlyTransactionSummary> MonthlyTransactions { get; set; } = new List<MonthlyTransactionSummary>();
        public List<TransactionTypeSummary> TransactionsByType { get; set; } = new List<TransactionTypeSummary>();
        public List<TopCustomerSummary> TopCustomers { get; set; } = new List<TopCustomerSummary>();
    }

    public class MonthlyTransactionSummary
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public string MonthName { get; set; } = string.Empty;
        public int TransactionCount { get; set; }
        public decimal TotalAmount { get; set; }
    }

    public class TransactionTypeSummary
    {
        public int TransactionType { get; set; }
        public string TypeName { get; set; } = string.Empty;
        public int Count { get; set; }
        public decimal TotalAmount { get; set; }
    }

    public class TopCustomerSummary
    {
        public string CustomerNIC { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public int TransactionCount { get; set; }
        public decimal TotalAmount { get; set; }
    }

    // Legacy DTOs for backward compatibility
    public class ReportDTO
    {
        public int CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerNIC { get; set; } = string.Empty;
        public List<LoanDTO> Loans { get; set; } = new List<LoanDTO>();
    }

    // Additional Report DTOs for TransactionService
    public class TransactionReportDTO
    {
        public int TransactionId { get; set; }
        public int CustomerId { get; set; }
        public string? CustomerName { get; set; }
        public string? CustomerNIC { get; set; }
        public decimal TotalAmount { get; set; }
        public int TransactionType { get; set; }
        public DateTime? CreatedAt { get; set; }
        public decimal? SubTotal { get; set; }
        public decimal? InterestRate { get; set; }
        public decimal? InterestAmount { get; set; }
        public CustomerReportDTO Customer { get; set; } = new CustomerReportDTO();
        public InvoiceReportDTO? Invoice { get; set; }
        public List<InstallmentReportDTO>? Installments { get; set; }
        public LoanReportDTO? Loan { get; set; }
        public List<LoanReportDTO> Loans { get; set; } = new List<LoanReportDTO>();
    }

    public class LoanReportDTO
    {
        public int LoanId { get; set; }
        public int? LoanPeriodId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public decimal? AmountPaid { get; set; }
        public decimal? OutstandingAmount { get; set; }
        public bool IsSettled { get; set; }
        public decimal? PrincipleAmount { get; set; }
        public decimal? InterestRate { get; set; }
        public decimal? InterestAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public int LoanPeriod { get; set; }
        public DateTime? CreatedAt { get; set; }
        public CustomerReportDTO Customer { get; set; } = new CustomerReportDTO();
    }

    public class InvoiceReportDTO
    {
        public int InvoiceId { get; set; }
        public string InvoiceNo { get; set; } = string.Empty;
        public DateTime? DateGenerated { get; set; }
        public int? Status { get; set; }
        public int InvoiceTypeId { get; set; }
        public decimal TotalAmount { get; set; }
        public CustomerReportDTO Customer { get; set; } = new CustomerReportDTO();
    }

    public class InstallmentReportDTO
    {
        public int InstallmentId { get; set; }
        public int? InstallmentNumber { get; set; }
        public decimal? AmountPaid { get; set; }
        public DateTime? DueDate { get; set; }
        public decimal InstallmentAmount { get; set; }
        public DateTime? PaymentDate { get; set; }
        public CustomerReportDTO Customer { get; set; } = new CustomerReportDTO();
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
