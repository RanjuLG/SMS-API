namespace SMS.Models.DTO
{
    public class GetItemDTO
    {
        public int ItemId { get; set; }
        public string? ItemDescription { get; set; }
        public string? ItemRemarks { get; set; }

        public decimal? ItemCaratage { get; set; }
        public decimal? ItemWeight { get; set; }
        public decimal? ItemGoldWeight { get; set; }
        public decimal? ItemValue { get; set; }
        public int? Status { get; set; }
        public DateTime? CreatedAt { get; set; }
        public string? CustomerNIC { get; set; } // New property
    }
    public class CreateItemDTO
    {
        //public int ItemId { get; set; }
        public string? ItemDescription { get; set; }
        public string? ItemRemarks { get; set; }
        public decimal? ItemWeight { get; set; }
        public decimal? ItemCaratage { get; set; }
        public decimal? ItemGoldWeight { get; set; }
        public decimal? ItemValue { get; set; }
        public int? Status { get; set; }
        public string? CustomerNIC { get; set; }
        //public DateTime? CreatedAt { get; set; } = DateTime.Now;
    }
    public class UpdateItemDTO
    {
        //public int ItemId { get; set; }
        public string? ItemDescription { get; set; }
        public string? ItemRemarks { get; set; }
        public decimal? ItemCaratage { get; set; }
        public decimal? ItemWeight { get; set; }
        public decimal? ItemGoldWeight { get; set; }
        public decimal? ItemValue { get; set; }
        public int? Status { get; set; }
        public string? CustomerNIC { get; set; }
        //public DateTime? UpdatedAt { get; set; } = DateTime.Now;
    }
    public class CommonItemDTO
    {
        public string? ItemDescription { get; set; }
        public string? ItemRemarks { get; set; }
        public decimal? ItemCaratage { get; set; }
        public decimal? ItemWeight { get; set; }
        public decimal? ItemGoldWeight { get; set; }
        public decimal? ItemValue { get; set; }
        public int? Status { get; set; }
    }

}
