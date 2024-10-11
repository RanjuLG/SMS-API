namespace SMS.Models.DTO
{
    public class GetCustomerDTO
    {
        public int CustomerId { get; set; }
        public string CustomerNIC { get; set; }
        public string? CustomerName { get; set; }
        public string? CustomerAddress { get; set; }
        public string? CustomerContactNo { get; set; }
        public DateTime? CreatedAt { get; set; }
        public string? NICPhotoPath { get; set; }  // Add NIC Photo Path

    }
    public class CreateCustomerDTO
    {
       // public int CustomerId { get; set; }
        public string CustomerNIC { get; set; }
        public string? CustomerName { get; set; }
        public string? CustomerAddress { get; set; }
        public string? CustomerContactNo { get; set; }
        public string? NICPhotoPath { get; set; }  // Add NIC Photo Path


    }
    public class UpdateCustomerDTO
    {
        // public int CustomerId { get; set; }
        public string CustomerNIC { get; set; }
        public string? CustomerName { get; set; }
        public string? CustomerAddress { get; set; }
        public string? CustomerContactNo { get; set; }
        public DateTime? UpdatedAt { get; set; } = DateTime.Now;
        public string? NICPhotoPath { get; set; }  // Add NIC Photo Path
    }

    public class CommonCustomerDTO
    {
        // public int CustomerId { get; set; }
        public string CustomerNIC { get; set; }
        public string? CustomerName { get; set; }
        public string? CustomerAddress { get; set; }
        public string? CustomerContactNo { get; set; }
        public string? NICPhotoPath { get; set; }  // Add NIC Photo Path
    }


}
