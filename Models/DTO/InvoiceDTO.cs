namespace SMS.Models.DTO
{
    namespace SMS.Models.DTO
    {
        public class InvoiceDTO
        {
            public int InvoiceId { get; set; }
            public string InvoiceNo { get; set; }
            public InvoiceType InvoiceTypeId { get; set; }
            public DateTime DateGenerated { get; set; }
            public string CustomerNIC { get; set; }
            public string ItemDescription { get; set; }
            public decimal Amount { get; set; }
            public bool PaymentStatus { get; set; }
            public decimal SubTotal { get; set; }
            public decimal Interest { get; set; }
            public decimal TotalAmount { get; set; }
           
            
        }
        public class GetInvoiceDTO
        {
            public int InvoiceId { get; set; }
            public string InvoiceNo { get; set; }
            public InvoiceType InvoiceTypeId { get; set; }
            public int TransactionId { get; set; }
            public string CustomerNIC { get; set; }
            public decimal? PrincipleAmount { get; set; }
            public decimal? InterestRate { get; set; }
            public decimal? InterestAmount { get; set; }
            public decimal TotalAmount { get; set; }
            public DateTime? DateGenerated { get; set; }
            public int? Status { get; set; }

            // New Property
            public int? LoanPeriod { get; set; } // e.g., "30 Days", "60 Days"
        }


        public class CreateInvoiceDTO
        {
            public InvoiceType InvoiceTypeId { get; set; }
            public CreateCustomerDTO Customer { get; set; }
           
            public CustomItemDTO[] Items { get; set; }

            public DateTime Date { get; set; }
            public bool PaymentStatus { get; set; }
           
            public decimal SubTotal { get; set; }
            public decimal InterestRate { get; set; }
            public decimal InterestAmount { get; set; }
            public decimal TotalAmount { get; set; }
            public int? LoanPeriodId { get; set; }

        }
        public class CreateInstallmentInvoiceDTO
        {
            public InvoiceType InvoiceTypeId { get; set; }
            public string InitialInvoiceNumber { get; set; }
            public CreateCustomerDTO Customer { get; set; }
           
            public CustomItemDTO[] Items { get; set; }

            public DateTime Date { get; set; }
            public bool PaymentStatus { get; set; }

            public decimal SubTotal { get; set; }
            public decimal Interest { get; set; }
            public decimal TotalAmount { get; set; }
            public int? LoanPeriodId { get; set; }

        }

        public class CustomItemDTO
        {
            public int itemId { get; set; }
            public string? ItemDescription { get; set; }
            public decimal? ItemCaratage { get; set; }
            public decimal? ItemGoldWeight { get; set; }
            public decimal? ItemValue { get; set; }

        }


        public class UpdateInvoiceDTO
        {
            public string InvoiceNo { get; set; }
            public string CustomerName { get; set; }
            public string CustomerAddress { get; set; }
            public string CustomerNIC { get; set; }
            public string ContactNo { get; set; }
            public string ItemDescription { get; set; }
            public decimal Amount { get; set; }
            public DateTime Date { get; set; }
            public bool PaymentStatus { get; set; }
            public decimal TotalAmount { get; set; }
            public decimal TotalGoldWeight { get; set; }
            public int Quantity { get; set; }
            public decimal SubTotal { get; set; }
            public decimal Interest { get; set; }
        }



        public class LoanInfo
        {
            public decimal? PrincipleAmount { get; set; }
            public decimal? InterestRate { get; set; }
            public decimal? InterestAmount { get; set; }
            public decimal TotalAmount { get; set; }
            public int LoanPeriod { get; set; }
            public int NumberOfInstallments { get; set; }
            public decimal InstallmentValue { get; set; }
            public int NumberOfInstallmentsPaid { get; set; }
            public int NumberOfInstallmentsToBePaid { get; set; }
            public bool IsLoanSettled { get; set; }
        }
    }
}