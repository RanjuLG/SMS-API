using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using SMS.Generic;
using SMS.Interfaces;
using SMS.Models;
using SMS.Models.DTO.SMS.Models.DTO;
using SMS.Models.DTO;

namespace SMS.Services
{
    public class TransactionService : ITransactionService
    {
        private readonly IRepository _dbContext;

        public TransactionService(IRepository dbContext)
        {
            _dbContext = dbContext;
        }

        public IList<TransactionReportDTO> GetAllTransactions(IDateTimeRange dateTimeRange)
        {
            var startTime = dateTimeRange.From;
            var endTime = dateTimeRange.To;

            var transactions = _dbContext.Get<Transaction>(i => i.DeletedAt == null && i.CreatedAt <= endTime && i.CreatedAt >= startTime)
                                         .Include(t => t.Invoice)
                                         .Include(t => t.Installments)
                                         .ToList();

            var transactionDTOs = transactions.Select(t => new TransactionReportDTO
            {
                TransactionId = t.TransactionId,
                TransactionType = t.TransactionType,
                CreatedAt = t.CreatedAt,
                SubTotal = t.SubTotal,
                InterestRate = t.InterestRate,
                InterestAmount = t.InterestAmount,
                TotalAmount = t.TotalAmount,
                Customer = t.Customer != null ? new CustomerReportDTO
                {
                    CustomerNIC = t.Customer.CustomerNIC,
                    CustomerName = t.Customer.CustomerName,
                    CustomerAddress = t.Customer.CustomerAddress,
                    CustomerContactNo = t.Customer.CustomerContactNo,
                } : null,
                Invoice = t.Invoice != null ? new InvoiceReportDTO
                {
                    InvoiceId = t.Invoice.InvoiceId,
                    InvoiceTypeId = t.Invoice.InvoiceTypeId,
                    InvoiceNo = t.Invoice.InvoiceNo,
                    DateGenerated = t.Invoice.DateGenerated,
                    Status = t.Invoice.Status
                } : null,
                Installments = t.Installments?.Select(i => new InstallmentReportDTO
                {
                    InstallmentId = i.InstallmentId,
                    InstallmentNumber = i.InstallmentNumber,
                    AmountPaid = i.AmountPaid,
                    DueDate = i.DueDate,
                    PaymentDate = i.PaymentDate
                }).ToList()
            }).ToList();

            return transactionDTOs;
        }


        public Transaction GetTransactionById(int transactionId)
        {
            return _dbContext.Get<Transaction>(t => t.TransactionId == transactionId && t.DeletedAt == null)
                             .Include(t => t.Customer)
                             .Include(t => t.Invoice)
                             .Include(t => t.TransactionItems)
                                 .ThenInclude(ti => ti.Item)
                             .FirstOrDefault();
        }

        public List<Transaction> GetTransactionsByIds(List<int> transactionIds)
        {
            return _dbContext.Get<Transaction>()
                             .Where(t => transactionIds.Contains(t.TransactionId) && t.DeletedAt == null)
                             .Include(t => t.Customer)
                             .Include(t => t.Invoice)
                             .Include(t => t.TransactionItems)
                                 .ThenInclude(ti => ti.Item)
                             .ToList();
        }

        public void CreateTransaction(Transaction transaction)
        {
            _dbContext.Create<Transaction>(transaction);
            _dbContext.Save();
        }

        public void UpdateTransaction(Transaction transaction)
        {
            transaction.UpdatedAt = DateTime.Now;
            _dbContext.Update<Transaction>(transaction);
            _dbContext.Save();
        }

        public void DeleteTransaction(int transactionId)
        {
            var transaction = _dbContext.GetById<Transaction>(transactionId);
            if (transaction != null)
            {
                transaction.DeletedAt = DateTime.Now;
                _dbContext.Update<Transaction>(transaction);
                _dbContext.Save();
            }
        }

        public void DeleteTransactions(IEnumerable<int> transactionIds)
        {
            var transactions = _dbContext.Get<Transaction>(t => transactionIds.Contains(t.TransactionId) && t.DeletedAt == null).ToList();
            foreach (var transaction in transactions)
            {
                transaction.DeletedAt = DateTime.Now;
                _dbContext.Update<Transaction>(transaction);
            }
            _dbContext.Save();
        }

        public IEnumerable<Transaction> GetTransactionsByCustomerId(int customerId)
        {
            return _dbContext.Get<Transaction>(t => t.CustomerId == customerId && t.DeletedAt == null)
                             .Include(t => t.Customer)
                             .Include(t => t.Invoice)
                             .Include(t => t.TransactionItems)
                                 .ThenInclude(ti => ti.Item)
                             .ToList();
        }
    }
}
