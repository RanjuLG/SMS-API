using SMS.Enums;
using SMS.Models;
using SMS.Models.DTO.SMS.Models.DTO;
using System.Collections.Generic;

namespace SMS.Interfaces
{
    public interface IInvoiceService
    {
        IList<Invoice> GetAllInvoices();
        IList<GetInvoiceDTO> GetInvoices();
        public GetInvoiceDTO GetInvoiceById(int invoiceId);
        void CreateInvoice(Invoice invoice);
        void UpdateInvoice(Invoice invoice);
        void DeleteInvoice(int invoiceId);
        void DeleteInvoices(IEnumerable<int> invoiceIds);
        public abstract string GenerateInvoiceNumber();
        public Invoice GetLastInvoice();
        IEnumerable<Invoice> GetInvoicesByCustomerId(int customerId);

    }
}
