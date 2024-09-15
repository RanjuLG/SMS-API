using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using SMS.Enums;
using SMS.Interfaces;
using SMS.Models;
using SMS.Models.DTO;

namespace SMS.Services
{
    public class LoanService : ILoanService
    {
        private readonly IRepository _dbContext;

        public LoanService(IRepository dbContext)
        {
            _dbContext = dbContext;
        }

        public Loan GetLoanById(int loanId)
        {
            return _dbContext.Get<Loan>(l => l.LoanId == loanId)
                             .Include(l => l.Transaction)
                             .Include(l => l.Installments)
                             .FirstOrDefault();
        }

        public IList<Loan> GetAllLoans()
        {
            return _dbContext.Get<Loan>()
                             .Include(l => l.Transaction)
                             .Include(l => l.Installments)
                             .ToList();
        }

        public IList<Loan> GetLoansByCustomerId(int customerId)
        {
            return _dbContext.Get<Loan>(l => l.Transaction.CustomerId == customerId)
                             .Include(l => l.Transaction)
                             .Include(l => l.Installments)
                             .ToList();
        }

        public void CreateLoan(Loan loanDto)
        {
            var loan = new Loan
            {
                TransactionId = loanDto.TransactionId,
                StartDate = loanDto.StartDate,
                EndDate = loanDto.EndDate
            };

            _dbContext.Create<Loan>(loan);
            _dbContext.Save();
        }

        public void UpdateLoan(Loan loanDto)
        {
            var loan = _dbContext.GetById<Loan>(loanDto.LoanId);
            if (loan != null)
            {
                loan.TransactionId = loanDto.TransactionId;
                loan.StartDate = loanDto.StartDate;
                loan.EndDate = loanDto.EndDate;

                _dbContext.Update<Loan>(loan);
                _dbContext.Save();
            }
        }

        public void DeleteLoan(int loanId)
        {
            var loan = _dbContext.GetById<Loan>(loanId);
            if (loan != null)
            {
                _dbContext.Delete<Loan>(loan);
                _dbContext.Save();
            }
        }
    }
}
