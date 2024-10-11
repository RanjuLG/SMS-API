using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SMS.Models
{
    public class Loan
    {
        [Key]
        public int LoanId { get; set; }

        [ForeignKey("LoanPeriod")]
        public int? LoanPeriodId { get; set; }
        // Foreign key to Transaction
        [ForeignKey("Transaction")]
        public int TransactionId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal AmountPaid { get; set; }
        public decimal OutstandingAmount {  get; set; }

        public Boolean IsSettled { get; set; }

        public DateTime? DeletedAt { get; set; }

        // Navigation Properties
        public virtual Transaction Transaction { get; set; }
        public virtual ICollection<Installment> Installments { get; set; }
        public virtual LoanPeriod LoanPeriod { get; set; }
    }
}
