namespace SMS.Models.DTO
{
    public class LoanPeriodDTO
    {
        public int Period { get; set; } // e.g., "30 Days", "60 Days"
    }

    public class KaratDTO
    {
      
        public int KaratValue { get; set; }
       

    }

    public class PricingDTO
    {
        public decimal Price { get; set; } // The price offering for this combination of Karat and LoanPeriod

        public int KaratId { get; set; }

        // Navigation property - make it virtual for lazy loading
       // public virtual Karat Karat { get; set; }

        public int LoanPeriodId { get; set; }

        // Navigation property - make it virtual for lazy loading
       // public virtual LoanPeriod LoanPeriod { get; set; }
    }

}
