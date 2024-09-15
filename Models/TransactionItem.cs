using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SMS.Models
{
    public class TransactionItem
    {
        [Key]
        public int TransactionItemId { get; set; }
        [ForeignKey("Transaction")]
        public int TransactionId { get; set; }
        [ForeignKey("Item")]
        public int ItemId { get; set; }

        public decimal? PawnValue { get; set; } // Capture pawn value at transaction time
        public DateTime? DeletedAt { get; set; }

        // Navigation Properties
        public virtual Transaction Transaction { get; set; }
        public virtual Item Item { get; set; }
    }
}
