using System;
using System.Collections.Generic;
using System.Linq;
using SMS.Interfaces;
using SMS.Models;

namespace SMS.Services
{
    public class TransactionItemService : ITransactionItemService
    {
        private readonly IRepository _dbContext;
        private readonly IReadOnlyRepository _readOnlyRepository;

        public TransactionItemService(IRepository dbContext, IReadOnlyRepository readOnlyRepository)
        {
            _dbContext = dbContext;
            _readOnlyRepository = readOnlyRepository;
        }

        public IList<TransactionItem> GetAllTransactionItems()
        {
            return _dbContext.Get<TransactionItem>(t => t.DeletedAt == null).ToList();
        }

        public IList<TransactionItem> GetTransactionItemsByTransactionId(int transactionId)
        {

            return _dbContext.Get<TransactionItem>(t => t.TransactionId == transactionId).ToList();
        }

        public TransactionItem GetTransactionItemById(int transactionItemId)
        {
            return _dbContext.Get<TransactionItem>(t => t.TransactionItemId == transactionItemId && t.DeletedAt == null).FirstOrDefault();
        }

        public void CreateTransactionItem(TransactionItem transactionItem)
        {
            using (var dbTransaction = _dbContext.CreateTransaction())
            {
                try
                {
                    _dbContext.Create<TransactionItem>(transactionItem);
                    _dbContext.Save();
                    _dbContext.CommitTransaction();
                }
                catch (Exception)
                {
                    _dbContext.RollbackTransaction();
                    throw;
                }
            }
        }

        public void UpdateTransactionItem(TransactionItem transactionItem)
        {
            using (var dbTransaction = _dbContext.CreateTransaction())
            {
                try
                {
                    _dbContext.Update<TransactionItem>(transactionItem);
                    _dbContext.Save();
                    _dbContext.CommitTransaction();
                }
                catch (Exception)
                {
                    _dbContext.RollbackTransaction();
                    throw;
                }
            }
        }

        public void DeleteTransactionItem(int transactionItemId)
        {
            using (var dbTransaction = _dbContext.CreateTransaction())
            {
                try
                {
                    var transactionItem = _dbContext.GetById<TransactionItem>(transactionItemId);
                    if (transactionItem != null)
                    {
                        transactionItem.DeletedAt = DateTime.Now;
                        _dbContext.Update<TransactionItem>(transactionItem);
                        _dbContext.Save();
                    }
                    _dbContext.CommitTransaction();
                }
                catch (Exception)
                {
                    _dbContext.RollbackTransaction();
                    throw;
                }
            }
        }

        public void DeleteTransactionItems(IEnumerable<int> transactionItemIds)
        {
            using (var dbTransaction = _dbContext.CreateTransaction())
            {
                try
                {
                    var transactionItems = _dbContext.Get<TransactionItem>(t => transactionItemIds.Contains(t.TransactionItemId) && t.DeletedAt == null).ToList();
                    foreach (var transactionItem in transactionItems)
                    {
                        transactionItem.DeletedAt = DateTime.Now;
                        _dbContext.Update<TransactionItem>(transactionItem);
                    }
                    _dbContext.Save();
                    _dbContext.CommitTransaction();
                }
                catch (Exception)
                {
                    _dbContext.RollbackTransaction();
                    throw;
                }
            }
        }
    }
}
