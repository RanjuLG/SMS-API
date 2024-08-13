using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SMS.Models
{
    public class Item
    {
        [Key]
        public int ItemId { get; set; }

        [ForeignKey("Customer")]
        public int? CustomerId { get; set; }
        public string? ItemDescription { get; set; }
        public decimal? ItemCaratage { get; set; }
        public decimal? ItemGoldWeight { get; set; }
        public decimal? ItemValue { get; set; }
        public int? Status { get; set; } = 0;
        public long? CreatedBy { get; set; }
        public long? UpdatedBy { get; set; }
        public long? DeletedBy { get; set; }
        public DateTime? CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; } = DateTime.Now;
        public DateTime? DeletedAt { get; set; }

        // Navigation Properties
        public virtual Customer Customer { get; set; }
        public virtual ICollection<TransactionItem> TransactionItems { get; set; }
    }
}
