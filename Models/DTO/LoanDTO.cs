namespace SMS.Models.DTO
{
    public class CreateLoanDTO
    {
        public int TransactionId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }

    public class UpdateLoanDTO
    {
        public int LoanId { get; set; }
        public int TransactionId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }

    public class LoanDTO
    {
        public int LoanId { get; set; }
        public int TransactionId { get; set; }
        public string InvoiceNo { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal AmountPaid { get; set; }
        public decimal OutstandingAmount { get; set; }
        public bool IsSettled { get; set; }
        public TransactionDTO Transaction { get; set; }
        public ICollection<InstallmentDTO> Installments { get; set; }
    }

    public class InstallmentDTO
    {
        public int InstallmentId { get; set; }

        public string InvoiceNo { get; set; }
        public int LoanId { get; set; }
        public decimal AmountPaid { get; set; }
        public DateTime DatePaid { get; set; }
    }
}
