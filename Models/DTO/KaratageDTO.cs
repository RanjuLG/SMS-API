using System.ComponentModel.DataAnnotations;

namespace SMS.Models.DTO
{
    public class GetKaratDTO
    {
        public int KaratId { get; set; }
        public int KaratValue { get; set; }
    }

    public class CreateKaratDTO
    {
        [Required]
        public int KaratValue { get; set; }
    }

    public class UpdateKaratDTO
    {
        [Required]
        public int KaratValue { get; set; }
    }

    public class GetLoanPeriodDTO
    {
        public int LoanPeriodId { get; set; }
        public int Period { get; set; }
    }

    public class CreateLoanPeriodDTO
    {
        [Required]
        public int Period { get; set; }
    }

    public class UpdateLoanPeriodDTO
    {
        [Required]
        public int Period { get; set; }
    }

    public class GetPricingDTO
    {
        public int PricingId { get; set; }
        public decimal Price { get; set; }
        public int KaratId { get; set; }
        public int LoanPeriodId { get; set; }
        public GetKaratDTO? Karat { get; set; }
        public GetLoanPeriodDTO? LoanPeriod { get; set; }
    }

    public class CreatePricingDTO
    {
        [Required]
        public decimal Price { get; set; }
        [Required]
        public int KaratId { get; set; }
        [Required]
        public int LoanPeriodId { get; set; }
    }

    public class UpdatePricingDTO
    {
        [Required]
        public decimal Price { get; set; }
    }

    public class PricingSearchRequest : PaginationRequest
    {
        public int? KaratId { get; set; }
        public int? LoanPeriodId { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
    }

    // Legacy DTOs for backward compatibility
    public class LoanPeriodDTO
    {
        public int Period { get; set; }
    }

    public class KaratDTO
    {
        public int KaratValue { get; set; }
    }

    public class PricingDTO
    {
        public decimal Price { get; set; }
        public int KaratId { get; set; }
        public int LoanPeriodId { get; set; }
    }

    public class PricingBatchDTO
    {
        public decimal Price { get; set; }
        public int KaratValue { get; set; }
        public int Period { get; set; }
    }

    public class PricingPutDTO
    {
        public decimal Price { get; set; }
    }
}
