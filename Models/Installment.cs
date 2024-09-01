using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SMS.Models
{
    public class Installment
    {
        [Key]
        public int InstallmentId { get; set; }

        public int InstallmentNumber { get; set; }

        [ForeignKey("Transaction")]
        public int TransactionId { get; set; }
        // Foreign key to Loan
        [ForeignKey("Loan")]
        public int LoanId { get; set; }
        public decimal AmountPaid { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime? PaymentDate { get; set; }

        // Navigation Properties
        public virtual Transaction Transaction { get; set; }
        public virtual Loan Loan { get; set; }
        DateTime? CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; } = DateTime.Now;
        public DateTime? DeletedAt { get; set; }
    }

}
