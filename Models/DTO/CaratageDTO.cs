namespace SMS.Models.DTO
{
    public class GetCaratageDTO
    {
        public int CaratageId { get; set; }
        public decimal Caratage { get; set; }
        public decimal AmountPerCaratage { get; set; }
    }

    public class CreateCaratageDTO
    {
        public decimal Caratage { get; set; }
        public decimal AmountPerCaratage { get; set; }
    }

    public class UpdateCaratageDTO
    {
        public decimal Caratage { get; set; }
        public decimal AmountPerCaratage { get; set; }
    }
}
