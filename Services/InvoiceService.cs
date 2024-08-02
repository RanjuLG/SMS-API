using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using SMS.Enums;
using SMS.Interfaces;
using SMS.Models;
using SMS.Models.DTO.SMS.Models.DTO;

namespace SMS.Services
{
    public class InvoiceService : IInvoiceService
    {
        private readonly IRepository _dbContext;
        private readonly ITransactionService _transactionService;

        public InvoiceService(IRepository dbContext,ITransactionService transactionService)
        {
            _dbContext = dbContext;
            _transactionService = transactionService;
        }

        public IList<Invoice> GetAllInvoices()
        {
            var invoices = _dbContext.Get<Invoice>(i => i.DeletedAt == null).Include(i => i.Transaction).ToList();


            return invoices;
         
        }

        public IList<GetInvoiceDTO> GetInvoices()
        {
            List<GetInvoiceDTO> invoices = [];

            var invoices_ = _dbContext.Get<Invoice>(i => i.DeletedAt == null).ToList();

            foreach (var invoice_ in invoices_)
            {
                var transaction = _transactionService.GetTransactionById(invoice_.TransactionId);
      
                var invoice = new GetInvoiceDTO
                {
                    InvoiceId = invoice_.InvoiceId,
                    InvoiceNo = invoice_.InvoiceNo,
                    TransactionId = transaction.TransactionId,
                    CustomerNIC = transaction.Customer?.CustomerNIC,
                    TotalAmount = transaction.TotalAmount,
                    DateGenerated = invoice_.DateGenerated,
                    Status = invoice_.Status,
                };

                invoices.Add(invoice);
            }


            return invoices;

        }

        public GetInvoiceDTO GetInvoiceById(int invoiceId)
        {
            List<GetInvoiceDTO> invoices = [];

            var invoice_ = _dbContext.GetById<Invoice>(invoiceId);

             var transaction = _transactionService.GetTransactionById(invoice_.TransactionId);

                var invoice = new GetInvoiceDTO
                {
                    InvoiceId = invoice_.InvoiceId,
                    InvoiceNo = invoice_.InvoiceNo,
                    TransactionId = transaction.TransactionId,
                    CustomerNIC = transaction.Customer?.CustomerNIC,
                    TotalAmount = transaction.TotalAmount,
                    DateGenerated = invoice_.DateGenerated,
                    Status = invoice_.Status,
                };

                invoices.Add(invoice);
            


            return invoice;
        }

        public void CreateInvoice(Invoice invoice)
        {
            _dbContext.Create<Invoice>(invoice);
            _dbContext.Save();
        }

        public void UpdateInvoice(Invoice invoice)
        {
            invoice.UpdatedAt = DateTime.Now;
            _dbContext.Update<Invoice>(invoice);
            _dbContext.Save();
        }

        public void DeleteInvoice(int invoiceId)
        {
           

            
            var invoice = _dbContext.GetById<Invoice>(invoiceId);

            Transaction transaction = null;
            if (invoice != null) 
            {
                transaction = _dbContext.GetById<Transaction>(invoice.TransactionId);
            
            }
           
            if (invoice != null && transaction != null)
            {
                invoice.DeletedAt = DateTime.Now;
                transaction.DeletedAt = DateTime.Now;
                _dbContext.Update<Invoice>(invoice);
                _dbContext.Update<Transaction>(transaction);
                _dbContext.Save();
            }
        }

        public void DeleteInvoices(IEnumerable<int> invoiceIds)
        {
            var invoices = _dbContext.Get<Invoice>(i => invoiceIds.Contains(i.InvoiceId) && i.DeletedAt == null).ToList();
            foreach (var invoice in invoices)
            {
                invoice.DeletedAt = DateTime.Now;
                _dbContext.Update<Invoice>(invoice);
            }
            _dbContext.Save();
        }


        public Invoice GetLastInvoice()
        {
            return _dbContext.Get<Invoice>(i => i.DeletedAt == null)?.OrderByDescending(i=> i.InvoiceId)?.FirstOrDefault();
        }

        public string GenerateInvoiceNumber()
        {
            var lastInvoice = GetLastInvoice();
            int nextInvoiceNumber = lastInvoice == null ? 1 : lastInvoice.InvoiceId + 1;
            return $"INVO{nextInvoiceNumber:D3}";
        }
    }
}
