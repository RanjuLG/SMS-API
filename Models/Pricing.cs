namespace SMS.Models
{
    public class Pricing
    {
        public int PricingId { get; set; }
        public decimal Price { get; set; } // The price offering for this combination of Karat and LoanPeriod

        public int KaratId { get; set; }

        // Navigation property - make it virtual for lazy loading
        public virtual Karat Karat { get; set; }

        public int LoanPeriodId { get; set; }

        // Navigation property - make it virtual for lazy loading
        public virtual LoanPeriod LoanPeriod { get; set; }
    }

}
