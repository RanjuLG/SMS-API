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

            if (!transactions.Any())
            {
                return new List<TransactionReportDTO>();
            }

            var transactionDTOs = transactions.Select(MapToTransactionReportDTO)
                                               .OrderByDescending(t => t.Invoice?.DateGenerated ?? DateTime.MinValue)
                                               .ToList();

            return transactionDTOs;
        }

        private TransactionReportDTO MapToTransactionReportDTO(Transaction transaction)
        {
            return new TransactionReportDTO
            {
                TransactionId = transaction.TransactionId,
                TransactionType = transaction.TransactionType,
                CreatedAt = transaction.CreatedAt,
                SubTotal = transaction.SubTotal,
                InterestRate = transaction.InterestRate,
                InterestAmount = transaction.InterestAmount,
                TotalAmount = transaction.TotalAmount,
                Customer = transaction.Customer != null ? MapToCustomerReportDTO(transaction.Customer) : null,
                Invoice = transaction.Invoice != null ? MapToInvoiceReportDTO(transaction.Invoice) : null,
                Installments = transaction.Installments?.Select(MapToInstallmentReportDTO).ToList(),
                Loan = transaction.Loan != null ? MapToLoanReportDTO(transaction.Loan) : null // Map Loan entity
            };
        }

        private LoanReportDTO MapToLoanReportDTO(Loan loan)
        {
            return new LoanReportDTO
            {
                LoanId = loan.LoanId,
                LoanPeriodId = loan.LoanPeriodId,
                StartDate = loan.StartDate,
                EndDate = loan.EndDate,
                AmountPaid = loan.AmountPaid,
                OutstandingAmount = loan.OutstandingAmount,
                IsSettled = loan.IsSettled
            };
        }

        private CustomerReportDTO MapToCustomerReportDTO(Customer customer)
        {
            return new CustomerReportDTO
            {
                CustomerNIC = customer.CustomerNIC,
                CustomerName = customer.CustomerName,
                CustomerAddress = customer.CustomerAddress,
                CustomerContactNo = customer.CustomerContactNo
            };
        }

        private InvoiceReportDTO MapToInvoiceReportDTO(Invoice invoice)
        {
            return new InvoiceReportDTO
            {
                InvoiceId = invoice.InvoiceId,
                InvoiceTypeId = invoice.InvoiceTypeId,
                InvoiceNo = invoice.InvoiceNo,
                DateGenerated = invoice.DateGenerated,
                Status = invoice.Status
            };
        }

        private InstallmentReportDTO MapToInstallmentReportDTO(Installment installment)
        {
            return new InstallmentReportDTO
            {
                InstallmentId = installment.InstallmentId,
                InstallmentNumber = installment.InstallmentNumber,
                AmountPaid = installment.AmountPaid,
                DueDate = installment.DueDate,
                PaymentDate = installment.PaymentDate
            };
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


        public decimal? GetRevenue()
        {
            decimal? revenue = 0;

            var interestTransactions = _dbContext.Get<Transaction>(t => t.DeletedAt == null && t.TransactionType == TransactionType.InstallmentPayment).Select(r=> r.InterestAmount);
            // Sum the InterestAmount values
            revenue = interestTransactions.Sum();

            return revenue;
        }
    }
}
