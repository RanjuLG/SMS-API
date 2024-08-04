using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using SMS.Interfaces;
using SMS.Models;

namespace SMS.Services
{
    public class TransactionService : ITransactionService
    {
        private readonly IRepository _dbContext;

        public TransactionService(IRepository dbContext)
        {
            _dbContext = dbContext;
        }

        public IList<Transaction> GetAllTransactions()
        {
            return _dbContext.Get<Transaction>(t => t.DeletedAt == null)
                             .Include(t => t.Customer)
                             .Include(t => t.Invoice)
                             .Include(t => t.TransactionItems)
                                 .ThenInclude(ti => ti.Item)
                             .ToList();
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
