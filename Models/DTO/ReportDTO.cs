namespace SMS.Models.DTO
{
    public class ReportDTO
    {
        public int CustomerId { get; set; }
        public string CustomerName { get; set; }
        public string CustomerNIC { get; set; }

        public decimal TotalLoanedAmount { get; set; }
        public decimal TotalAmountPaid { get; set; }
        public decimal TotalOutstandingAmount { get; set; }

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
}
