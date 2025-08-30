using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using SMS.Enums;
using SMS.Interfaces;
using SMS.Models;
using SMS.Models.DTO;
using SMS.Repositories;

namespace SMS.Services
{
    public class InvoiceService : IInvoiceService
    {
        private readonly IRepository _dbContext;
        private readonly ITransactionService _transactionService;
        private readonly ILoanService _loanService;
        private readonly ITransactionItemService _transactionItemService;
        private readonly IInstallmentService _installmentService;
        private readonly ILogger<InvoiceService> _logger;

        public InvoiceService(IRepository dbContext, ITransactionService transactionService, ILoanService loanService,ITransactionItemService transactionItemService,IInstallmentService installmentService, ILogger<InvoiceService> logger)
        {
            _dbContext = dbContext;
            _transactionService = transactionService;
            _loanService = loanService;
            _transactionItemService = transactionItemService;
            _installmentService = installmentService;
            _logger = logger;
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
                .Include(i => i.Transaction) // Ensure Transaction is include
                .ToList();

            foreach (var invoice_ in invoices_)
            {
                var transaction = _transactionService.GetTransactionById(invoice_.TransactionId);

                if (transaction == null)
                {
                    continue;
                }

                var loans = _loanService.GetAllLoans(dateTimeRange);

                var invoice = new GetInvoiceDTO
                {
                    InvoiceId = invoice_.InvoiceId,
                    InvoiceTypeId = (int)invoice_.InvoiceTypeId,
                    InvoiceNo = invoice_.InvoiceNo,
                    TransactionId = transaction.TransactionId,
                    CustomerNIC = transaction.Customer?.CustomerNIC,
                    TotalAmount = transaction.TotalAmount,
                    DateGenerated = invoice_.DateGenerated,
                    Status = invoice_.Status,
                   LoanPeriod = (int)invoice_.InvoiceTypeId == (int)InvoiceType.InitialPawnInvoice ? loans.Where(t => t.TransactionId == invoice_.TransactionId).FirstOrDefault()?.LoanPeriod?.Period : null,
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
                InvoiceTypeId = (int)invoice_.InvoiceTypeId,
                InvoiceNo = invoice_.InvoiceNo,
                TransactionId = transaction?.TransactionId ?? 0,
                CustomerNIC = transaction?.Customer?.CustomerNIC,
                TotalAmount = transaction?.TotalAmount ?? 0,
                DateGenerated = invoice_.DateGenerated,
                Status = invoice_.Status,
               //LoanPeriod = transaction.LoanPeriod?.Period // Map LoanPeriod
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
            if (invoiceId <= 0)
            {
                _logger.LogWarning($"Invalid Invoice ID: {invoiceId}");
                throw new ArgumentException("Invalid Invoice ID");
            }

            var invoice = _dbContext.GetById<Invoice>(invoiceId); // Using repository for fetching invoice
            if (invoice == null)
            {
                _logger.LogWarning($"Invoice with ID {invoiceId} not found.");
                return;
            }

            try
            {
                _dbContext.CreateTransaction(); // Start transaction

                if (invoice.InvoiceTypeId == InvoiceType.InitialPawnInvoice)
                {
                    if (DeleteInitialInvoice(invoice))
                    {
                        _logger.LogInformation($"Invoice {invoiceId} deleted successfully.");
                    }
                    else
                    {
                        _logger.LogWarning($"Failed to delete invoice {invoiceId}.");
                    }
                }

                _dbContext.CommitTransaction(); // Commit transaction
            }
            catch (Exception ex)
            {
                _dbContext.RollbackTransaction(); // Rollback transaction in case of error
                _logger.LogError(ex, $"Error while deleting invoice {invoiceId}");
                throw;
            }
        }


        private bool DeleteInitialInvoice(Invoice initialInvoice)
        {
            bool isDeleted = false;

            var transaction = _dbContext.GetById<Transaction>(initialInvoice.TransactionId);
            if (transaction == null)
            {
                _logger.LogWarning($"Transaction with ID {initialInvoice.TransactionId} not found.");
                return false;
            }

            try
            {
                // Delete Transaction Items
                var transactionItems = _transactionItemService
                    .GetTransactionItemsByTransactionId(transaction.TransactionId)
                    .Select(t => t.TransactionItemId)
                    .ToList();

                if (transactionItems.Any())
                {
                    _transactionItemService.DeleteTransactionItems(transactionItems);
                }

                // Delete Loan
                var loan = _loanService.GetLoanByInitialInvoiceNumber(initialInvoice.InvoiceNo);
                if (loan != null)
                {
                    _loanService.DeleteLoan(loan.LoanId);
                }

                // Delete Installments and related data
                var installments = _installmentService
                    .GetInstallmentsByInitialInvoiceNumber(initialInvoice.InvoiceNo)
                    .ToList();

                if (installments.Any())
                {
                    _installmentService.DeleteInstallments(installments.Select(i => i.InstallmentId));

                    var installmentTransactions = installments.Select(i => i.TransactionId).ToList();
                    var installmentInvoices = _dbContext
                        .Get<Invoice>(i => i.DeletedAt == null && installmentTransactions.Contains(i.TransactionId))
                        .ToList();

                    DeleteInvoices(installmentInvoices.Select(i => i.InvoiceId));
                    _transactionService.DeleteTransactions(installmentTransactions);
                }

                // Mark invoice and transaction as deleted
                initialInvoice.DeletedAt = DateTime.UtcNow;
                transaction.DeletedAt = DateTime.UtcNow;
                _dbContext.Update(initialInvoice);
                _dbContext.Update(transaction);
                _dbContext.Save();

                isDeleted = true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error occurred while deleting invoice {initialInvoice.InvoiceId}");
                throw;
            }

            return isDeleted;
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
            string todaysDate = DateTime.Today.ToString("yyyyMMdd");

            return $"GC{todaysDate}{nextInvoiceNumber}";
        }

        public IEnumerable<Invoice> GetInvoicesByCustomerId(int customerId)
        {


            return _dbContext.Get<Invoice>(i => i.Transaction.CustomerId == customerId && i.DeletedAt == null).ToList();
        }

        public IEnumerable<Invoice> GetInvoiceByInvoiceNo(string invoiceNo)
        {
            return _dbContext.Get<Invoice>(i => i.InvoiceNo.ToLower() == invoiceNo.ToLower() && i.DeletedAt == null).ToList();
        }


        public int? GetInvoiceCount()
        {


            return _dbContext.Get<Invoice>(i =>  i.DeletedAt == null).Count();
        }
    }
}
