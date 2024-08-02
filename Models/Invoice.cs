using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SMS.Models
{
    public class Invoice
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int InvoiceId { get; set; }

        public string? InvoiceNo { get; set; }

        [ForeignKey("Transaction")]
        public int TransactionId { get; set; }

        public DateTime? DateGenerated { get; set; } = DateTime.Now;
        public int? Status { get; set; } = 1;
        public long? CreatedBy { get; set; }
        public long? UpdatedBy { get; set; }
        public long? DeletedBy { get; set; }
        public DateTime? CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; } = DateTime.Now;
        public DateTime? DeletedAt { get; set; }

        // Navigation property for the related Transaction
        public virtual Transaction? Transaction { get; set; }
    }
}
