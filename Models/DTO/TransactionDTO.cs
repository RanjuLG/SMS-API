using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace SMS.Models.DTO
{
    public class TransactionDTO
    {
        public int TransactionId { get; set; }

        public DateTime? CreatedAt { get; set; }
        public decimal SubTotal { get; set; }
        public decimal Interest { get; set; }
        public decimal TotalAmount { get; set; }

        public GetCustomerDTO Customer { get; set; }
        public GetItemDTO Item { get; set; }
    }


    public class CreateTransactionDTO
    {
        public string CustomerName { get; set; }
        public string CustomerAddress { get; set; }
        public string CustomerNIC { get; set; }
        public string ContactNo { get; set; }
        public string? ItemDescription { get; set; }
        public decimal? ItemCaratage { get; set; }
        public decimal? ItemGoldWeight { get; set; }
        public decimal? ItemValue { get; set; }

        public DateTime Date { get; set; }
        public bool PaymentStatus { get; set; }

        public decimal SubTotal { get; set; }
        public decimal Interest { get; set; }
        public decimal TotalAmount { get; set; }

    }

    public class UpdateTransactionDTO
    {
        public string Type { get; set; }
        public string Description { get; set; }
        public decimal Amount { get; set; }
        public DateTime Date { get; set; }
    }
}
