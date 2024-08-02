using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SMS.Models
{
    public class Item
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ItemId { get; set; }

        public string? ItemDescription { get; set; }
        public decimal? ItemCaratage { get; set; }
        public decimal? ItemGoldWeight { get; set; }
        public decimal? ItemValue { get; set; }
        public int? Status { get; set; }
        public long? CreatedBy { get; set; }
        public long? UpdatedBy { get; set; }
        public long? DeletedBy { get; set; }
        public DateTime? CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; } = DateTime.Now;
        public DateTime? DeletedAt { get; set; }

        [ForeignKey("Customer")]
        public int? CustomerId { get; set; }

        // Navigation property for the related Customer
        public virtual Customer? Customer { get; set; }

        public virtual ICollection<TransactionItem> TransactionItems { get; set; }
    }
}
