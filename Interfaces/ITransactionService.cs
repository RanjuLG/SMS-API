using SMS.Enums;
using SMS.Models;
using SMS.Models.DTO;
using System.Collections.Generic;

namespace SMS.Interfaces
{
    public interface ITransactionService
    {
        IList<TransactionReportDTO> GetAllTransactions(IDateTimeRange dateTimeRange);
        Transaction GetTransactionById(int transactionId);
        List<Transaction> GetTransactionsByIds(List<int> transactionIds);
        void CreateTransaction(Transaction transaction);
        void UpdateTransaction(Transaction transaction);
        void DeleteTransaction(int transactionId);
        void DeleteTransactions(IEnumerable<int> transactionIds);
        IEnumerable<Transaction> GetTransactionsByCustomerId(int customerId);
        decimal? GetRevenue();
        int GetTransactionCount();
    }
}
