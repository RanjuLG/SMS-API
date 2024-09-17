using SMS.Models;
using SMS.Models.DTO;
using System;
using System.Collections.Generic;

namespace SMS.Interfaces
{
    public interface ILoanService
    {
        Loan GetLoanById(int loanId);
        IList<Loan> GetAllLoans();
        IList<Loan> GetLoansByCustomerId(int customerId);
        Loan GetLoanByInitialInvoiceNumber(string initialInvoiceNumber);
        void CreateLoan(Loan loanDto);
        void UpdateLoan(Loan loanDto);
        void DeleteLoan(int loanId);
    }
}
