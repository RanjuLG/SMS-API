using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SMS.Models
{
    public class Loan
    {
        [Key]
        public int LoanId { get; set; }

        // Foreign key to Transaction
        [ForeignKey("Transaction")]
        public int TransactionId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public Boolean IsSettled { get; set; }

        // Navigation Properties
        public virtual Transaction Transaction { get; set; }
        public virtual ICollection<Installment> Installments { get; set; }
    }
}
