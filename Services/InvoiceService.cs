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

        public InvoiceService(IRepository dbContext, ITransactionService transactionService)
        {
            _dbContext = dbContext;
            _transactionService = transactionService;
        }

        public IList<Invoice> GetAllInvoices()
        {
            var invoices = _dbContext.Get<Invoice>(i => i.DeletedAt == null)
                .Include(i => i.Transaction)
                .Include(i => i.InvoiceTypeId)
                .ToList();

            return invoices;
        }

        public IList<GetInvoiceDTO> GetInvoices(IDateTimeRange dateTimeRange)
        {
            var startTime = dateTimeRange.From;
            var endTime = dateTimeRange.To;

            var invoices = new List<GetInvoiceDTO>();
            var invoices_ = _dbContext.Get<Invoice>(i => i.DeletedAt == null && i.CreatedAt <= endTime && i.CreatedAt >= startTime)
                .Include(i => i.Transaction) // Ensure Transaction is included
                .ThenInclude(t => t.LoanPeriod) // Ensure LoanPeriod is included
                .ToList();

            foreach (var invoice_ in invoices_)
            {
                var transaction = _transactionService.GetTransactionById(invoice_.TransactionId);

                if (transaction == null)
                {
                    continue;
                }

                var invoice = new GetInvoiceDTO
                {
                    InvoiceId = invoice_.InvoiceId,
                    InvoiceTypeId = invoice_.InvoiceTypeId,
                    InvoiceNo = invoice_.InvoiceNo,
                    TransactionId = transaction.TransactionId,
                    CustomerNIC = transaction.Customer?.CustomerNIC,
                    TotalAmount = transaction.TotalAmount,
                    DateGenerated = invoice_.DateGenerated,
                    Status = invoice_.Status,
                    LoanPeriod = transaction.LoanPeriod?.Period // Map LoanPeriod
                };

                invoices.Add(invoice);
            }

            return invoices;
        }

        public GetInvoiceDTO GetInvoiceById(int invoiceId)
        {
            var invoice_ = _dbContext.GetById<Invoice>(invoiceId);
            if (invoice_ == null)
            {
                return null;
            }

            var transaction = _transactionService.GetTransactionById(invoice_.TransactionId);
            var invoice = new GetInvoiceDTO
            {
                InvoiceId = invoice_.InvoiceId,
                InvoiceTypeId = invoice_.InvoiceTypeId,
                InvoiceNo = invoice_.InvoiceNo,
                TransactionId = transaction.TransactionId,
                CustomerNIC = transaction.Customer?.CustomerNIC,
                TotalAmount = transaction.TotalAmount,
                DateGenerated = invoice_.DateGenerated,
                Status = invoice_.Status,
                LoanPeriod = transaction.LoanPeriod?.Period // Map LoanPeriod
            };

            return invoice;
        }


        public void CreateInvoice(Invoice invoice)
        {
            _dbContext.Create(invoice);
            _dbContext.Save();
        }

        public void UpdateInvoice(Invoice invoice)
        {
            invoice.UpdatedAt = DateTime.Now;
            _dbContext.Update(invoice);
            _dbContext.Save();
        }

        public void DeleteInvoice(int invoiceId)
        {
            var invoice = _dbContext.GetById<Invoice>(invoiceId);
            if (invoice == null) return;

            var transaction = _dbContext.GetById<Transaction>(invoice.TransactionId);
            if (transaction == null) return;

            invoice.DeletedAt = DateTime.Now;
            transaction.DeletedAt = DateTime.Now;
            _dbContext.Update(invoice);
            _dbContext.Update(transaction);
            _dbContext.Save();
        }

        public void DeleteInvoices(IEnumerable<int> invoiceIds)
        {
            var invoices = _dbContext.Get<Invoice>(i => invoiceIds.Contains(i.InvoiceId) && i.DeletedAt == null).ToList();
            foreach (var invoice in invoices)
            {
                invoice.DeletedAt = DateTime.Now;
                _dbContext.Update(invoice);
            }
            _dbContext.Save();
        }

        public Invoice GetLastInvoice()
        {
            return _dbContext.Get<Invoice>(i => i.DeletedAt == null)
                .OrderByDescending(i => i.InvoiceId)
                .FirstOrDefault();
        }

        public string GenerateInvoiceNumber()
        {
            var lastInvoice = GetLastInvoice();
            int nextInvoiceNumber = lastInvoice == null ? 1 : lastInvoice.InvoiceId + 1;
            return $"INVO{nextInvoiceNumber:D3}";
        }

        public IEnumerable<Invoice> GetInvoicesByCustomerId(int customerId)
        {
            return _dbContext.Get<Invoice>(i => i.Transaction.CustomerId == customerId && i.DeletedAt == null).ToList();
        }

        public IEnumerable<Invoice> GetInvoiceByInvoiceNo(string invoiceNo)
        {
            return _dbContext.Get<Invoice>(i => i.InvoiceNo.ToLower() == invoiceNo.ToLower() && i.DeletedAt == null).ToList();
        }
    }
}
