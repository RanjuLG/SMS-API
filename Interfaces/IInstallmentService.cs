using System;
using System.Collections.Generic;
using SMS.Models;

namespace SMS.Interfaces
{
    public interface IInstallmentService
    {
        IList<Installment> GetAllInstallments();

        Installment GetInstallmentById(int installmentId);


        IList<Installment> GetInstallmentsByTransactionId(int transactionId);
        void CreateInstallment(Installment installment);

        void UpdateInstallment(Installment installment);


        void DeleteInstallment(int installmentId);

        void DeleteInstallments(IEnumerable<int> installmentIds);

        IList<Installment> GetInstallmentsByInitialInvoiceNumber(string invoiceNumber);

        IList<Installment> GetInstallmentsForCustomer(int customerId);
    }
}
