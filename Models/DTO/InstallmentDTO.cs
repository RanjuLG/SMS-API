using System.ComponentModel.DataAnnotations.Schema;

namespace SMS.Models.DTO
{
    public class GetInstallmentDTO
    {
        public int InstallmentId { get; set; }

        public int TransactionId { get; set; }

        public decimal AmountDue { get; set; }
        public decimal AmountPaid { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime? PaymentDate { get; set; } // Nullable to represent unpaid installments

        public int InstallmentNumber { get; set; }


    }
    public class CreateInstallmentDTO
    {
        public int InstallmentId { get; set; }

        public int TransactionId { get; set; }

        public decimal AmountDue { get; set; }
        public decimal AmountPaid { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime? PaymentDate { get; set; }

        public DateTime? CreatedAt { get; set; } = DateTime.Now;


    }
    public class UpdateInstallmentDTO
    {
        public int InstallmentId { get; set; }

        public int TransactionId { get; set; }

        public decimal AmountDue { get; set; }
        public decimal AmountPaid { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime? PaymentDate { get; set; }

        public DateTime? UpdatedAt { get; set; }
    }

    public class CommonInstallmentDTO
    {
        public int InstallmentId { get; set; }

        public int TransactionId { get; set; }

        public decimal AmountDue { get; set; }
        public decimal AmountPaid { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime? PaymentDate { get; set; }
    }
}
