namespace SMS.Models
{
    public class LoanPeriod
    {
        public int LoanPeriodId { get; set; }
        public int Period { get; set; } // e.g., "30 Days", "60 Days"
       // public virtual ICollection<Pricing> Pricings { get; set; }
    }

}
