using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SMS.Models
{
    public class Invoice
    {
        [Key]
        public int InvoiceId { get; set; }
        [ForeignKey("InvoiceTypes")]
        public InvoiceType InvoiceTypeId { get; set; }

        [ForeignKey("Transaction")]
        public int TransactionId { get; set; }
        public string InvoiceNo { get; set; }
        public DateTime? DateGenerated { get; set; }
        public int? Status { get; set; }
        public long? CreatedBy { get; set; }
        public long? UpdatedBy { get; set; }
        public long? DeletedBy { get; set; }
        public DateTime? CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; } = DateTime.Now;
        public DateTime? DeletedAt { get; set; }

        // Navigation Properties
        //public virtual InvoiceType InvoiceType { get; set; }
        public virtual Transaction Transaction { get; set; }
    }
}


public enum InvoiceType
{
    InitialPawnInvoice = 1,
    InstallmentPaymentInvoice = 2,
    SettlementInvoice = 3
}