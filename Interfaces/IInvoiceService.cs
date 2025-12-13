using SMS.Enums;
using SMS.Models;
using SMS.Models.DTO;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SMS.Interfaces
{
    public interface IInvoiceService
    {
        IList<Invoice> GetAllInvoices();
        IList<GetInvoiceDTO> GetInvoices(IDateTimeRange dateTimeRange);
        Task<PaginatedResponse<GetInvoiceDTO>> GetInvoicesPaginatedAsync(InvoiceSearchRequest request);
        public GetInvoiceDTO GetInvoiceById(int invoiceId);
        void CreateInvoice(Invoice invoice);
        void UpdateInvoice(Invoice invoice);
        void DeleteInvoice(int invoiceId);
        void DeleteInvoices(IEnumerable<int> invoiceIds);
        public abstract string GenerateInvoiceNumber();
        public Invoice GetLastInvoice();
        IEnumerable<Invoice> GetInvoicesByCustomerId(int customerId);
        IEnumerable<Invoice> GetInvoiceByInvoiceNo(string invoiceNo);
        int? GetInvoiceCount();

    }
}
