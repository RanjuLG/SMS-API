using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SMS.Models
{
    public class Transaction
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int TransactionId { get; set; }

        [ForeignKey("Customer")]
        public int CustomerId { get; set; }
        public decimal? SubTotal { get; set; }
        public decimal? InterestRate { get; set; }
        public decimal? TotalAmount { get; set; }
        public long? CreatedBy { get; set; }
        public long? UpdatedBy { get; set; }
        public long? DeletedBy { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }

        // Navigation property for the related Customer
        public virtual Customer? Customer { get; set; }

        // Navigation property for the related Item
        public virtual Item? Item { get; set; }

        // Navigation property for the related Invoice
        public virtual Invoice? Invoice { get; set; }
        public virtual ICollection<TransactionItem> TransactionItems { get; set; }
    }
}
