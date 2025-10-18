using System.ComponentModel.DataAnnotations;

namespace SMS.Models.DTO
{
    public class GetCustomerDTO
    {
        public int CustomerId { get; set; }
        public string CustomerNIC { get; set; } = string.Empty;
        public string? CustomerName { get; set; }
        public string? CustomerAddress { get; set; }
        public string? CustomerContactNo { get; set; }
        public DateTime? CreatedAt { get; set; }
        public string? NICPhotoPath { get; set; }
    }

    public class CreateCustomerDTO
    {
        [Required]
        public string CustomerNIC { get; set; } = string.Empty;
        public string? CustomerName { get; set; }
        public string? CustomerAddress { get; set; }
        public string? CustomerContactNo { get; set; }
    }

    public class UpdateCustomerDTO
    {
        [Required]
        public string CustomerNIC { get; set; } = string.Empty;
        public string? CustomerName { get; set; }
        public string? CustomerAddress { get; set; }
        public string? CustomerContactNo { get; set; }
    }

    public class CustomerSearchRequest : DateRangeRequest
    {
        public string? CustomerNIC { get; set; }
    }
}
