using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SMS.Models
{
    // Define the enum outside the class
    public enum InvoiceTypess
    {
        InitialPawnInvoice = 1,
        InstallmentPaymentInvoice = 2,
        SettlementInvoice = 3
    }

    public class InvoiceTypes
    {
        [Key]
        public int InvoiceTypeId { get; set; }

        // This property will store the specific number for the invoice type
        public int InvoiceTypeNumber { get; set; }

        // This property will store the name of the invoice type
        [Required]
        [StringLength(50)] // Adjust the length based on your needs
        public string InvoiceTypeName { get; set; }

        // Use the enum for InvoiceType property
        [NotMapped]
        public InvoiceType InvoiceType
        {
            get { return (InvoiceType)InvoiceTypeNumber; }
            set
            {
                InvoiceTypeNumber = (int)value;
                InvoiceTypeName = value.ToString(); // Set the name based on the enum value
            }
        }
    }
}
