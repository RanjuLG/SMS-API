using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using SMS.Interfaces;
using SMS.Models;

namespace SMS.Services
{
    public class InstallmentService : IInstallmentService
    {
        private readonly IRepository _dbContext;

        public InstallmentService(IRepository dbContext)
        {
            _dbContext = dbContext;
        }

        public IList<Installment> GetAllInstallments()
        {
            return _dbContext.Get<Installment>(i => i.DeletedAt == null)
                             .Include(i => i.Transaction)
                             .ToList();
        }

        public Installment GetInstallmentById(int installmentId)
        {
            return _dbContext.Get<Installment>(i => i.InstallmentId == installmentId && i.DeletedAt == null)
                             .Include(i => i.Transaction)
                             .FirstOrDefault();
        }

        public IList<Installment> GetInstallmentsByTransactionId(int transactionId)
        {
            return _dbContext.Get<Installment>(i => i.TransactionId == transactionId && i.DeletedAt == null)
                             .Include(i => i.Transaction)
                             .ToList();
        }

        public void CreateInstallment(Installment installment)
        {
            _dbContext.Create<Installment>(installment);
            _dbContext.Save();
        }

        public void UpdateInstallment(Installment installment)
        {
            installment.UpdatedAt = DateTime.Now;
            _dbContext.Update<Installment>(installment);
            _dbContext.Save();
        }

        public void DeleteInstallment(int installmentId)
        {
            var installment = _dbContext.GetById<Installment>(installmentId);
            if (installment != null)
            {
                installment.DeletedAt = DateTime.Now;
                _dbContext.Update<Installment>(installment);
                _dbContext.Save();
            }
        }

        public void DeleteInstallments(IEnumerable<int> installmentIds)
        {
            var installments = _dbContext.Get<Installment>(i => installmentIds.Contains(i.InstallmentId) && i.DeletedAt == null).ToList();
            foreach (var installment in installments)
            {
                installment.DeletedAt = DateTime.Now;
                _dbContext.Update<Installment>(installment);
            }
            _dbContext.Save();
        }

        public IList<Installment> GetInstallmentsByInitialInvoiceNumber(string invoiceNumber)
        {
            return _dbContext.Get<Installment>(i => i.Loan.Transaction.Invoice.InvoiceNo == invoiceNumber && i.DeletedAt == null)
                             .Include(i => i.Transaction)
                             .ToList();
        }
    }
}
