using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SMS.Models
{
    public class Installment
    {
        [Key]
        public int InstallmentId { get; set; }

        [ForeignKey("Transaction")]
        public int TransactionId { get; set; }

        public decimal AmountDue { get; set; }
        public decimal AmountPaid { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime? PaymentDate { get; set; } // Nullable to represent unpaid installments

        // Navigation Properties
        public virtual Transaction Transaction { get; set; }

        DateTime? CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; } = DateTime.Now;
        public DateTime? DeletedAt { get; set; }
    }

}
