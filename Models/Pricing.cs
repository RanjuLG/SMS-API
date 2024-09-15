using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace SMS.Models
{
    public class Pricing
    {
        [Key]
        public int PricingId { get; set; }
        public decimal Price { get; set; } // The price offering for this combination of Karat and LoanPeriod

        [ForeignKey("Karat")]
        public int KaratId { get; set; }
        public virtual Karat Karat { get; set; }

        [ForeignKey("LoanPeriod")]
        public int LoanPeriodId { get; set; }
        public virtual LoanPeriod LoanPeriod { get; set; }
    }

}
