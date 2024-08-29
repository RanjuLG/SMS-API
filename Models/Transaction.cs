using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SMS.Models
{
    public class Transaction
    {
        [Key]
        public int TransactionId { get; set; }

        [ForeignKey("Customer")]
        public int CustomerId { get; set; }

        public decimal? SubTotal { get; set; }
        public decimal? InterestRate { get; set; }
        public decimal? TotalAmount { get; set; }

        // New Field
        [ForeignKey("LoanPeriod")]
        public int? LoanPeriodId { get; set; }
        public TransactionType TransactionType { get; set; }
        public long? CreatedBy { get; set; }
        public long? UpdatedBy { get; set; }
        public long? DeletedBy { get; set; }
        public DateTime? CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; } = DateTime.Now;
        public DateTime? DeletedAt { get; set; }

        // Navigation Properties
        public virtual Customer Customer { get; set; }
        public virtual LoanPeriod LoanPeriod { get; set; }
        public virtual ICollection<TransactionItem> TransactionItems { get; set; }
        public virtual Invoice Invoice { get; set; }
        public virtual ICollection<Installment> Installments { get; set; }
    }

}


public enum TransactionType
{
    LoanIssuance = 1,
    InstallmentPayment = 2,
    InterestPayment = 3,
    LateFeePayment = 4,
    LoanClosure = 5
}
