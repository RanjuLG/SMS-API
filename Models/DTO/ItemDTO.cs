using System.ComponentModel.DataAnnotations;

namespace SMS.Models.DTO
{
    public class GetItemDTO
    {
        public int ItemId { get; set; }
        public int CustomerId { get; set; }
        public string? CustomerName { get; set; }
        public string? CustomerNIC { get; set; }
        public string? ItemDescription { get; set; }
        public decimal? ItemCaratage { get; set; }
        public decimal? ItemGoldWeight { get; set; }
        public decimal? ItemValue { get; set; }
        public decimal? ItemWeight { get; set; }
        public string? ItemRemarks { get; set; }
        public DateTime? CreatedAt { get; set; }
        public int? Status { get; set; }  // Legacy property for backward compatibility
    }

    public class CreateItemDTO
    {
        [Required]
        public int CustomerId { get; set; }
        public string? ItemDescription { get; set; }
        public decimal? ItemCaratage { get; set; }
        public decimal? ItemGoldWeight { get; set; }
        public decimal? ItemValue { get; set; }
        public decimal? ItemWeight { get; set; }
        public string? ItemRemarks { get; set; }
        
        // Legacy properties for backward compatibility
        public string? CustomerNIC { get; set; }
        public int? Status { get; set; }
    }

    public class UpdateItemDTO
    {
        [Required]
        public int CustomerId { get; set; }
        public string? ItemDescription { get; set; }
        public decimal? ItemCaratage { get; set; }
        public decimal? ItemGoldWeight { get; set; }
        public decimal? ItemValue { get; set; }
        public decimal? ItemWeight { get; set; }
        public string? ItemRemarks { get; set; }
        
        // Legacy properties for backward compatibility
        public string? CustomerNIC { get; set; }
        public int? Status { get; set; }
    }

    public class ItemSearchRequest : PaginationRequest
    {
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
        public string? CustomerNIC { get; set; }
        public decimal? MinValue { get; set; }
        public decimal? MaxValue { get; set; }
    }

    // Legacy DTOs for backward compatibility
    public class CustomItemDTO
    {
        public int itemId { get; set; }  // Legacy property name (lowercase)
        public string? ItemDescription { get; set; }
        public decimal? ItemCaratage { get; set; }
        public decimal? ItemGoldWeight { get; set; }
        public decimal? ItemValue { get; set; }
        public decimal? ItemWeight { get; set; }
        public string? ItemRemarks { get; set; }
        public decimal? PawnValue { get; set; }
    }
}
